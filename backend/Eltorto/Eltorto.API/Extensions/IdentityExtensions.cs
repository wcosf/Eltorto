using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Eltorto.API.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}