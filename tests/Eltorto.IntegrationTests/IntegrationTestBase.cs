using System.Net.Http.Headers;
using System.Net.Http.Json;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Eltorto.Domain.Abstractions;
using Eltorto.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Eltorto.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected AppDbContext? DbContext;
    protected readonly PostgreSqlContainer Container;
    private readonly WebApplicationFactory<Program> _factory;

    protected IntegrationTestBase()
    {
        Container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("eltorto_test")
            .WithUsername("postgres")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>((sp, options) =>
                    {
                        options.UseNpgsql(Container.GetConnectionString());
                    });
                });
            });

        Client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var scope = _factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbContext.Database.MigrateAsync();

        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.CreateAdminIfNotExistsAsync();

        await SeedDataAsync(scope);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
        await _factory.DisposeAsync();
    }

    private async Task SeedDataAsync(IServiceScope scope)
    {
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (!await unitOfWork.Categories.ExistsBySlugAsync("classic"))
        {
            var category = new Category { Slug = "classic", Name = "Классические", SortOrder = 1, Description = "Классические торты" };
            await unitOfWork.Categories.AddAsync(category);
            await unitOfWork.SaveChangesAsync();
        }

        if (!await unitOfWork.Fillings.ExistsAsync(f => f.Name == "Сливочный"))
        {
            var filling = new Filling
            {
                Name = "Сливочный",
                Description = "Нежный сливочный крем",
                ImageUrl = "https://example.com/filling1.jpg",
                HasCrossSection = false
            };
            await unitOfWork.Fillings.AddAsync(filling);
            await unitOfWork.SaveChangesAsync();
        }

        var classicCategory = await unitOfWork.Categories.GetBySlugAsync("classic");
        var creamFilling = (await unitOfWork.Fillings.FindAsync(f => f.Name == "Сливочный")).FirstOrDefault();
        if (classicCategory != null && creamFilling != null)
        {
            if (!await unitOfWork.Cakes.ExistsAsync(c => c.Name == "Наполеон"))
            {
                var cake = new Cake
                {
                    Name = "Наполеон",
                    ImageUrl = "https://example.com/napoleon.jpg",
                    ThumbnailUrl = "https://example.com/napoleon_thumb.jpg",
                    CategorySlug = "classic",
                    IsFeatured = true,
                    Description = "Слоёный торт с заварным кремом",
                    FillingId = creamFilling.Id
                };
                await unitOfWork.Cakes.AddAsync(cake);
                await unitOfWork.SaveChangesAsync();
            }
        }

        if (!await unitOfWork.ContactSettings.ExistsAsync(cs => true))
        {
            var settings = new ContactSettings
            {
                Phone = "+7 (123) 456-78-90",
                AdditionalPhone = "+7 (098) 765-43-21",
                Email = "info@eltorto.ru",
                Address = "г. Москва, ул. Примерная, д. 1",
                MapUrl = "https://maps.example.com/eltorto"
            };
            await unitOfWork.ContactSettings.AddAsync(settings);
            await unitOfWork.SaveChangesAsync();
        }

        var aboutPage = await unitOfWork.Pages.GetBySlugAsync("about");
        if (aboutPage == null)
        {
            aboutPage = new Page
            {
                Slug = "about",
                Title = "О нас",
                MetaDescription = "Страница о кондитерской",
                Heading = "Добро пожаловать!",
                Subheading = "Мы печём с любовью",
                Content = "Текст о нас..."
            };
            await unitOfWork.Pages.AddAsync(aboutPage);
            await unitOfWork.SaveChangesAsync();

            var block = new ContentBlock
            {
                PageId = aboutPage.Id,
                Title = "Наша история",
                Text = "Мы начали свой путь в 2020 году...",
                ImageUrl = "https://example.com/history.jpg",
                SortOrder = 1
            };
            await unitOfWork.ContentBlocks.AddAsync(block);
            await unitOfWork.SaveChangesAsync();
        }

        if (!await unitOfWork.Orders.ExistsAsync(o => o.CustomerPhone == "+7 (999) 111-22-33"))
        {
            var cake = (await unitOfWork.Cakes.FindAsync(c => c.Name == "Наполеон")).FirstOrDefault();
            var filling = (await unitOfWork.Fillings.FindAsync(f => f.Name == "Сливочный")).FirstOrDefault();

            var order = new Order
            {
                CreatedAt = DateTime.UtcNow,
                CustomerName = "Иван Петров",
                CustomerPhone = "+7 (999) 111-22-33",
                CustomerEmail = "ivan@example.com",
                CakeId = cake?.Id,
                FillingId = filling?.Id,
                Weight = 2.5m,
                DeliveryDate = DateTime.UtcNow.AddDays(3),
                DeliveryAddress = "г. Москва, ул. Ленина, д. 10, кв. 5",
                Status = "New",
                Comment = "Позвонить за час до доставки"
            };
            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveChangesAsync();
        }

        if (!await unitOfWork.Sliders.ExistsAsync(s => true))
        {
            var slide = new SliderItem
            {
                ImageUrl = "https://example.com/slide1.jpg",
                Title = "Первый слайд",
                Subtitle = "Добро пожаловать в Eltorto!",
                SortOrder = 1
            };
            await unitOfWork.Sliders.AddAsync(slide);
            await unitOfWork.SaveChangesAsync();
        }

        if (!await unitOfWork.Testimonials.ExistsAsync(t => true))
        {
            var testimonial1 = new Testimonial
            {
                Date = DateTime.UtcNow.AddDays(-5),
                Author = "Анна Смирнова",
                Email = "anna@example.com",
                Text = "Очень вкусный торт! Заказывали на день рождения, всем понравился. Спасибо!",
                IsApproved = true,
                Response = "Спасибо за отзыв! Рады, что вам понравилось."
            };
            await unitOfWork.Testimonials.AddAsync(testimonial1);

            var testimonial2 = new Testimonial
            {
                Date = DateTime.UtcNow.AddDays(-2),
                Author = "Петр Иванов",
                Email = "petr@example.com",
                Text = "Отличная кондитерская! Буду заказывать еще.",
                IsApproved = false,
                Response = null
            };
            await unitOfWork.Testimonials.AddAsync(testimonial2);

            await unitOfWork.SaveChangesAsync();
        }
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            UserName = "admin",
            Password = "Admin123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.AccessToken;
    }

    protected async Task<string> CreateAndLoginCustomerAsync()
    {
        var userName = $"customer_{Guid.NewGuid():N}";
        var registerDto = new RegisterRequest
        {
            UserName = userName,
            Email = $"{userName}@test.ru",
            Password = "Customer123!"
        };
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { UserName = userName, Password = "Customer123!" });
        loginResponse.EnsureSuccessStatusCode();
        var token = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return token!.AccessToken;
    }

    protected async Task<HttpResponseMessage> AuthorizedRequestAsync(
        HttpMethod method,
        string url,
        object? content = null,
        string? token = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (content != null)
            request.Content = JsonContent.Create(content);
        return await Client.SendAsync(request);
    }
}