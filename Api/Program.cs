var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

// endpoints
app.MapGet("/", () => "waguri says hello!");

app.MapGet("/test/{id}", (int id) =>
{
    return Results.Ok(id);
});

app.Run();
