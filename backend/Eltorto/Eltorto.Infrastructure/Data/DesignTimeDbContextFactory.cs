using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Eltorto.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../Eltorto.API"));
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        DbContextOptionsConfigurator.ConfigureNpgsql(
            optionsBuilder,
            connectionString,
            typeof(AppDbContext).Assembly.FullName);

        return new AppDbContext(optionsBuilder.Options);
    }
}