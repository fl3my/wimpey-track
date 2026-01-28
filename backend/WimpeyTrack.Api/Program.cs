using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Data.Seed;
using WimpeyTrack.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

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

// Add the PDF conversion service
builder.Services.AddScoped<IPdfConverterService, PdfConverterService>();

// Add the Receipt Analysis endpoint
builder.Services.AddScoped<IReceiptAnalysisService, ReceiptAnalysisService>();

// Add the receipt imge storage service
builder.Services.AddScoped<IReceiptImageStorage, FileSystemReceiptImageStorage>();

// Add the image processing service
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Add custom vision service
builder.Services.AddHttpClient<IVisionService, VisionService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["VisionService:BaseUrl"] ?? throw new InvalidOperationException()
    );
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
db.Database.Migrate();

// Seed data
await DatabaseInitializer.InitializeAsync(app.Services);

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();