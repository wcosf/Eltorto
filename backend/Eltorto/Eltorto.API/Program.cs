using AutoMapper;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Application.Mapping;
using Eltorto.Application.Services;
using Eltorto.Infrastructure;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICakeService, CakeService>();
builder.Services.AddScoped<IFillingService, FillingService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ISliderService, SliderService>();
builder.Services.AddScoped<IContactSettingsService, ContactSettingsService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // ТОЛЬКО EF Core миграции (структура таблиц)
        await services.ApplyMigrationsAsync();

        // ПОКА ЗАКОММЕНТИРУЙТЕ миграцию данных
        //await services.MigrateDataAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration");
    }
}

app.Run();