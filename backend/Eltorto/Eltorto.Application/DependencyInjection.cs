using Eltorto.Application.Interfaces.Services;
using Eltorto.Application.Mapping;
using Eltorto.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eltorto.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICakeService, CakeService>();
        services.AddScoped<IFillingService, FillingService>();
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPageService, PageService>();
        services.AddScoped<ISliderService, SliderService>();
        services.AddScoped<IContactSettingsService, ContactSettingsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}