using System.Text.Json.Serialization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Data.Seed;
using WimpeyTrack.Api.Domain;
using WimpeyTrack.Api.Providers;
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

// Add scoped services
builder.Services.AddScoped<IPdfConverterService, PdfConverterService>();
builder.Services.AddScoped<IReceiptAnalysisService, ReceiptAnalysisService>();
builder.Services.AddScoped<IReceiptImageStorage, FileSystemReceiptImageStorage>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IReportGenerationService, ReportGenerationService>();
builder.Services.AddScoped<IReceiptProvider, ReceiptProvider>();
builder.Services.AddScoped<IExpenseWorkbookBuilder, ExpenseWorkbookBuilder>();
builder.Services.AddScoped<IBookBuilder, BookBuilder>();
builder.Services.AddScoped<IReportZipBuilder, ReportZipBuilder>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportStorageService, ReportStorageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IJourneyDistanceService, JourneyDistanceService>();
builder.Services.AddScoped<IPreferenceProvider, PreferenceProvider>();
builder.Services.AddScoped<IProfileProvider, ProfileProvider>();



// Add custom vision service
builder.Services.AddHttpClient<IVisionService, VisionService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["VisionService:BaseUrl"] ?? throw new InvalidOperationException()
    );
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
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

// Add compatibility for pmtiles
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".pmtiles"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.MapFallbackToFile("index.html");

app.UseAuthorization();

app.MapControllers();

app.Run();