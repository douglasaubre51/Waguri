using Api.Data;
using Api.Dtos;
using Api.Dtos.AiraDtos;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Api.Wrappers;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// session
builder.Services.AddSession();

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add mvc controllers
builder.Services.AddControllersWithViews();

// load env variables
DotNetEnv.Env.Load();

// add connection to waguridb
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WaguriDataString")
        )
    );

// add identity framework
builder.Services
    .AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// jwt auth
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<JwtTokenProvider>();

// add repositories
builder.Services.AddScoped<ClientRepository>();

// add session service
builder.Services.AddScoped<SessionService>();

// add auth service
builder.Services.AddScoped<AuthService>();

// add client storage service
builder.Services.AddScoped<ClientStorageService>();


var app = builder.Build();

// https
app.UseHttpsRedirection();

// css,js etc 
app.UseStaticFiles();

// api routing
app.UseRouting();

// auth and session
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// swagger
app.UseSwagger();
app.UseSwaggerUI();

// mvc controller routing
app.MapControllers();

// default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
    );



// AIRA endpoints

// start
app.MapGet("/", () => "waguri says hello!");

// waguri status
app.MapGet("/hello", () => "live");

// find user
app.MapGet(
    "/user/find/{emailId}",
    async (
       [FromRoute] string emailId,
       [FromServices] UserManager<User> _userManager
        ) =>
{
    try
    {
        User? user = await _userManager.FindByEmailAsync(emailId);
        if (user is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"findUser error: {ex}");
        return Results.InternalServerError();
    }
});

// get all user account details
app.MapGet("/get-all-users",
    async (
        [FromServices] ApplicationDbContext dbContext
        ) =>
    {
        try
        {
            // get user details from Users table
            var users = await dbContext.Users.ToListAsync();
            List<UserDto> dto = [];
            foreach (var u in users)
            {
                dto.Add(new UserDto
                {
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed,
                    ProjectId = u.ProjectId
                });
            }

            UserDtoList dtoList = new()
            {
                Users = dto
            };
            return Results.Ok(dtoList);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Results.InternalServerError();
        }
    });

// login
app.MapPost(
    "/login",
    async (
     [FromBody] LoginDto dto,
     [FromServices] UserManager<User> _userManager,
     [FromServices] SignInManager<User> _signInManager,
     [FromServices] JwtTokenProvider _jwtTokenProvider
    ) =>
    {
        try
        {
            Debug.WriteLine($"Email: {dto.Email}");
            Debug.WriteLine($"Password: {dto.Password}");

            var result = await _signInManager.PasswordSignInAsync(
                dto.Email,
                dto.Password,
                false,
                false
                );
            if (result.Succeeded is false)
            {
                Debug.WriteLine($"invalid login attempt by : {dto.Email}");
                return Results.NotFound($"invalid login attempt by : {dto.Email}");
            }
            // success
            Debug.WriteLine($"user {dto.Email} logged in!");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            var token = _jwtTokenProvider.CreateToken(user);

            return Results.Ok(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex}");
            return Results.InternalServerError();
        }
    });

// sign up
app.MapPost(
    "/sign-up",
    async (
    [FromBody] SignUpDto dto,
    [FromServices] UserManager<User> _userManager
    ) =>
{
    try
    {
        Debug.WriteLine($"username: {dto.Email}");

        var user = new User
        {
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            ProjectId = dto.ProjectId
        };

        IdentityResult? result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded is false)
        {
            Debug.WriteLine($"account for {dto.Email} couldnot be created!");
            foreach (var e in result.Errors)
                Debug.WriteLine(e.Description);
            return Results.BadRequest(result.Errors);
        }

        // send confirmation email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"http://localhost:5000/sign-up/confirm/{user.Id}/{code}";
        var appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD");
        var senderMail = "douglasaubre@gmail.com";
        var senderName = "douglas aubre";
        // craft email
        var email = new MimeMessage();
        email.From.Add(
            new MailboxAddress(senderName, senderMail)
            );
        email.To.Add(
            new MailboxAddress(user.FirstName, user.Email)
            );
        email.Subject = "Waguri account confirmation";
        email.Body = new TextPart("html")
        {
            Text = $"<h1>Confirmation code :</h1><strong>{callbackUrl}</strong><br>only lasts for a day!"
        };
        // mail client
        Debug.WriteLine($"sending email to : {user.Email}");
        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(
            "smtp.gmail.com",
            587,
            MailKit.Security.SecureSocketOptions.StartTls
            );
        await smtpClient.AuthenticateAsync(
            senderMail,
            appPassword
            );
        await smtpClient.SendAsync(email);
        Debug.WriteLine($"email has been sent to : {user.Email}");
        await smtpClient.DisconnectAsync(true);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"SignIn error:\n{ex}");
        return Results.InternalServerError();
    }
});

// trigger confirm user account
app.MapGet(
    "/user/confirm/{userName}",
    async (
        [FromRoute] string userName,
        [FromServices] UserManager<User> _userManager,
        [FromServices] AuthService _authService
        ) =>
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            await _authService.GetConfirmationEmail(user);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ConfirmUser error:\n{ex}");
            return Results.InternalServerError();
        }
    });

// delete user account
app.MapGet("/user/delete/{UserName}",
   async (
       [FromRoute] string UserName,
       [FromServices] UserManager<User> _userManager
    ) =>
{
    try
    {
        var user = await _userManager.FindByNameAsync(UserName);
        if (user is null)
            return Results.NotFound($"account {UserName} not found!");

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded is false)
            return Results.BadRequest("account {UserName} couldnt be deleted!");

        // success
        return Results.Ok($"account {UserName} deleted!");
    }
    catch (Exception ex)
    {
        return Results.InternalServerError(ex);
    }
});


app.Run();
