using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add routing service
var osrmBaseUrl = builder.Configuration["OsrmSettings:BaseUrl"] 
                  ?? throw new InvalidOperationException("OsrmSettings:BaseUrl is required");
builder.Services.AddHttpClient<IRouteService, RouteService>(client =>
{
    client.BaseAddress = new Uri(osrmBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();