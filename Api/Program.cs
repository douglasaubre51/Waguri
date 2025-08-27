using Api.Data;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// add connection to waguridb
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WaguriDataString")
        )
    );

// add identity framework
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = true
        )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
//app.UseAuthorization();

// endpoints
app.MapGet("/", () => "waguri says hello!");

// /sign-up
app.MapPost("/sign-up", async (
    [FromBody] SignUpDto dto,
    UserManager<User> _userManager
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
            ProjectId = dto.ProjectId
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded is false)
        {
            Debug.WriteLine($"account for {dto.UserName} couldnot be created!");
            return Results.BadRequest();
        }
        // success

        return Results.Created();
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"SignIn error:\n{ex}");
        return Results.InternalServerError();
    }
});

app.Run();
