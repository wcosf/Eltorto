using Eltorto.Domain.Abstractions;
using Eltorto.Domain.Repositories;
using Eltorto.Infrastructure.Data;
using Eltorto.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eltorto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            DbContextOptionsConfigurator.ConfigureNpgsql(
                options,
                connectionString,
                typeof(AppDbContext).Assembly.FullName);
        });

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICakeRepository, CakeRepository>();
        services.AddScoped<IContactSettingsRepository, ContactSettingsRepository>();
        services.AddScoped<IFillingRepository, FillingRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISliderRepository, SliderRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}