using Api.Dtos.AiraDtos;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.Name = ".WAGURI.Session";
});

// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add mvc controllers
builder.Services.AddControllersWithViews();

// load env variables; remove in production!
// Env.Load();

string connectionString = Environment.GetEnvironmentVariable("WAGURI_DB_STRING");

// add connection to waguridb
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("WAGURI_DB_STRING"),
        o => o.EnableRetryOnFailure()
        )
    );

// add identity framework
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
    })
.AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// jwt auth
builder.Services.AddAuthentication()
    .AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenProvider>();

// add repositories
builder.Services.AddScoped<ClientRepository>();

// add storage services
builder.Services.AddSingleton<ConnectionStorageService>();

// add hub services
builder.Services.AddTransient<NativeAuth>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<ClientStorageService>();

var app = builder.Build();

// https
// disable so non https clients can connect! like springboot!
//app.UseHttpsRedirection();

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

// Add SignalR hubs
app.MapHub<NativeAuthHub>("/native-auth");

// default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
    );


// AIRA endpoints

// waguri status

app.MapGet("/hello", () => Results.Ok("WAGURI is running..."));

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
                return Results.NotFound();

            return Results.Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"aira userFind error: {ex.Message}");
            return Results.InternalServerError(ex.Message);
        }
    });

// get all users

app.MapGet(
    "/user/all",
    async (

        [FromServices] ApplicationDbContext dbContext
        ) =>
    {
        try
        {
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
            Console.WriteLine("aira getAllUsers: " + ex.Message);
            return Results.InternalServerError(ex.Message);
        }
    });

// create user

app.MapPost(
    "/user/create",
    async (

        [FromBody] SignUpDto dto,

        [FromServices] UserManager<User> _userManager,

        [FromServices] AuthService _authService
        ) =>
    {
        try
        {

            Console.WriteLine($"creating user ...");
            Console.WriteLine($"user password: {dto.Password}");

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
                Console.WriteLine($"account for {dto.Email} couldnot be created!");
                return Results.BadRequest(result.Errors);
            }

            await _authService.GetConfirmationEmail(user);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"aira userCreate error: {ex.Message}");
            return Results.InternalServerError(ex.Message);
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
            Console.WriteLine($"aira ConfirmUser error: {ex.Message}");
            return Results.InternalServerError(ex.Message);
        }
    });

// delete user account

app.MapGet(
    "/user/delete/{email}",
    async (
        [FromRoute] string email,
        [FromServices] UserManager<User> _userManager
        ) =>
    {
        try
        {
            var user = await _userManager.FindByNameAsync(email);
            if (user is null)
                return Results.NotFound($"account {email} not found!");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded is false)
                return Results.BadRequest("account {email} couldnt be deleted!");

            return Results.Ok($"account {email} deleted!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("aira userDelete error: " + ex.Message);
            return Results.InternalServerError(ex.Message);
        }
    });

// project endpoints

app.MapGet("/project/all",
    (

     [FromServices] ClientRepository _clientRepo
    ) =>
    {
        try
        {

            ClientDtoList dto = new();
            dto.Clients = _clientRepo.GetAll();

            return Results.Ok(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllProjects error: {ex.Message}");
            return Results.InternalServerError(ex.Message);
        }
    }
    );

app.MapPost("/project/create",
    (
     [FromBody] Api.Dtos.ClientDto dto,
     [FromServices] ClientRepository _clientRepo
    ) =>
    {
        try
        {
            Console.WriteLine("Project Id: " + dto.ProjectId);
            Console.WriteLine("Api Url: " + dto.ApiUrl);
            Console.WriteLine("Url: " + dto.Url);

            var client = new Client
            {
                ApiUrl = dto.ApiUrl,
                Url = dto.Url,
                ProjectId = dto.ProjectId
            };

            _clientRepo.Create(client);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateProjects error: " + ex.Message);
            return Results.InternalServerError();
        }
    }
);

app.MapGet(
    "/project/remove/{id}",
    (
     [FromRoute] String id,
     [FromServices] ClientRepository _clientRepo
    ) =>
    {
        try
        {
            _clientRepo.RemoveByProjectId(id);

            return Results.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("RemoveProject error: " + ex.Message);
            return Results.InternalServerError();
        }
    }
);


app.Run();
