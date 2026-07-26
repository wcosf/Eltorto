using Eltorto.API.Extensions;
using Eltorto.API.Middleware;
using Eltorto.API.Exceptions;
using Eltorto.Application;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Infrastructure;
using Eltorto.Infrastructure.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearer();

builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Eltorto.Application.Validators.RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["https://eltorto.ru", "https://www.eltorto.ru"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add rate limited policies
builder.Services.AddRateLimitingPolicies();

var app = builder.Build();

// Create directory for uploading images
var uploadSettings = app.Configuration.GetSection("UploadSettings");
var basePath = uploadSettings["BasePath"] ?? "uploads";
var subPaths = new[]
{
    uploadSettings["FillingsSubPath"] ?? "fillings",
    uploadSettings["PortfolioSubPath"] ?? "portfolio",
    uploadSettings["SliderSubPath"] ?? "slider",
    uploadSettings["PagesSubPath"] ?? "pages"
};

var fullBasePath = Path.Combine(Directory.GetCurrentDirectory(), basePath);
if (!Directory.Exists(fullBasePath))
    Directory.CreateDirectory(fullBasePath);

foreach (var sub in subPaths)
{
    var fullSubPath = Path.Combine(fullBasePath, sub);
    if (!Directory.Exists(fullSubPath))
        Directory.CreateDirectory(fullSubPath);
}

// Apply migrations and seed admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var authService = services.GetRequiredService<IAuthService>();
        await authService.CreateAdminIfNotExistsAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration/seeding");
    }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("Frontend");
app.UseExceptionHandler();
app.UseAuthentication();
app.UseMiddleware<CsrfMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();