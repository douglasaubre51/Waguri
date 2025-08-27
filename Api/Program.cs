using Api.Data;
using Api.Dtos;
using Api.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

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
    .AddIdentity<User, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = true
        )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
//app.UseAuthorization();


// endpoints
app.MapGet("/", () => "waguri says hello!");

// /sign-up
app.MapPost(
    "/sign-up",
    async (
    [FromBody] SignUpDto dto,
    [FromServices] UserManager<User> _userManager
    ) =>
{
    try
    {
        Debug.WriteLine($"username: {dto.UserName}");

        var user = new User
        {
            UserName = dto.UserName,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            ProjectId = dto.ProjectId
        };

        IdentityResult? result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded is false)
        {
            Debug.WriteLine($"account for {dto.UserName} couldnot be created!");
            foreach (var e in result.Errors)
                Debug.WriteLine(e.Description);
            return Results.BadRequest(result.Errors);
        }

        // send confirmation email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = $"https://localhost:7140/sign-up/confirm/{user.Id}/{code}";
        var appPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD");
        var senderMail = "douglasaubre@gmail.com";
        var senderName = "douglas aubre";
        // craft email
        var email = new MimeMessage();
        email.From.Add(
            new MailboxAddress(senderName, senderMail)
            );
        email.To.Add(
            new MailboxAddress(user.UserName, user.Email)
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

// /sign-up/confirm/{userId}/{code}
app.MapGet(
    "/sign-up/confirm/{userId}/{code}",
    async (
    [FromRoute] string userId,
    [FromRoute] string code,
    [FromServices] UserManager<User> _userManager
    ) =>
{
    try
    {
        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            Debug.WriteLine("user doesnot exists!");
            return Results.NotFound("user doesnot exists!");
        }
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded is false)
        {
            Debug.WriteLine($"couldn't confirm email for : {user.UserName}");
            return Results.Unauthorized();
        }

        return Results.Ok("email confirmed!");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"ConfirmSignUp error:\n{ex}");
        return Results.InternalServerError();
    }
});

app.Run();
