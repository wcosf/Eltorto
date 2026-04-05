using Dapper;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.DataMigration.Mappings;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;

namespace Eltorto.Infrastructure.DataMigration.Migrations;

public class Migration_2026_03_20_InitialData : IMigration
{
    private readonly string _mysqlConnectionString;
    private readonly string _postgresConnectionString;
    private readonly ILogger<Migration_2026_03_20_InitialData> _logger;

    public string Name => "2026_03_20_InitialData";
    public int Order => 1;

    public Migration_2026_03_20_InitialData(
        string mysqlConnectionString,
        string postgresConnectionString,
        ILogger<Migration_2026_03_20_InitialData> logger)
    {
        _mysqlConnectionString = mysqlConnectionString;
        _postgresConnectionString = postgresConnectionString;
        _logger = logger;
    }

    public async Task UpAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting data migration from MySQL to PostgreSQL...");

        using var mysqlConn = new MySqlConnection(_mysqlConnectionString);
        using var postgresConn = new NpgsqlConnection(_postgresConnectionString);

        await postgresConn.OpenAsync(cancellationToken);

        await using var transaction = await postgresConn.BeginTransactionAsync(cancellationToken);

        try
        {
            await ClearPostgresTablesAsync(postgresConn, transaction, cancellationToken);

            await MigrateCategoriesAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigrateFillingsAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigrateCakesAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigrateTestimonialsAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigratePagesAndBlocksAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigrateSliderAsync(mysqlConn, postgresConn, transaction, cancellationToken);
            await MigrateContactsAsync(mysqlConn, postgresConn, transaction, cancellationToken);

            await postgresConn.ExecuteAsync(@"
                INSERT INTO ""__MigrationsHistory"" (""MigrationName"", ""AppliedDate"", ""Details"")
                VALUES (@Name, @Date, @Details)",
                new { Name, Date = DateTime.UtcNow, Details = "Initial data migration from MySQL" },
                transaction);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Data migration completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data migration failed! Rolling back...");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task ClearPostgresTablesAsync(NpgsqlConnection postgresConn, NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing existing data...");

        await postgresConn.ExecuteAsync("SET session_replication_role = 'replica';", transaction: transaction);

        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"ContentBlocks\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Orders\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Cakes\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Testimonials\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Fillings\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Categories\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Pages\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"SliderItems\" RESTART IDENTITY CASCADE;", transaction: transaction);
        await postgresConn.ExecuteAsync("TRUNCATE TABLE \"ContactSettings\" RESTART IDENTITY CASCADE;", transaction: transaction);

        await postgresConn.ExecuteAsync("SET session_replication_role = 'origin';", transaction: transaction);

        _logger.LogInformation("Existing data cleared.");
    }

    private async Task MigrateCategoriesAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating categories...");

        var categories = await mysqlConn.QueryAsync<MySqlCategory>(new CommandDefinition(@"
            SELECT id, cat, catrus, content, sorto FROM cat",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var cat in categories)
        {
            var sortOrder = int.TryParse(cat.sorto, out var sort) ? sort : 0;

            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""Categories"" (""Id"", ""Slug"", ""Name"", ""Description"", ""SortOrder"")
                VALUES (@Id, @Slug, @Name, @Description, @SortOrder)",
                new
                {
                    Id = cat.id,
                    Slug = cat.cat,
                    Name = cat.catrus,
                    Description = string.IsNullOrEmpty(cat.content) ? null : cat.content,
                    SortOrder = sortOrder
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""Categories_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Categories""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} categories.", count);
    }

    private async Task MigrateFillingsAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating fillings...");

        var fillings = await mysqlConn.QueryAsync<MySqlFilling>(new CommandDefinition(@"
            SELECT id, nazv, nach, kart, razrez FROM nachinka",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var filling in fillings)
        {
            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""Fillings"" (""Id"", ""Name"", ""Description"", ""ImageUrl"", ""HasCrossSection"")
                VALUES (@Id, @Name, @Description, @ImageUrl, @HasCrossSection)",
                new
                {
                    Id = filling.id,
                    Name = filling.nazv ?? "Без названия",
                    Description = filling.nach,
                    ImageUrl = filling.kart,
                    HasCrossSection = filling.razrez == "1"
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""Fillings_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Fillings""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} fillings.", count);
    }

    private async Task MigrateCakesAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating cakes...");

        var cakes = await mysqlConn.QueryAsync<MySqlCake>(new CommandDefinition(@"
            SELECT id, smfoto, foto, category, podcategory, luch, nazvanie, commo FROM kart",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var cake in cakes)
        {
            int? fillingId = null;

            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""Cakes"" (
                    ""Id"", ""Name"", ""ImageUrl"", ""ThumbnailUrl"", 
                    ""CategorySlug"", ""SubCategory"", ""IsFeatured"", 
                    ""Description"", ""FillingId""
                )
                VALUES (
                    @Id, @Name, @ImageUrl, @ThumbnailUrl, 
                    @CategorySlug, @SubCategory, @IsFeatured, 
                    @Description, @FillingId
                )",
                new
                {
                    Id = cake.id,
                    Name = cake.nazvanie,
                    ImageUrl = cake.foto,
                    ThumbnailUrl = cake.smfoto,
                    CategorySlug = cake.category,
                    SubCategory = string.IsNullOrEmpty(cake.podcategory) ? null : cake.podcategory,
                    IsFeatured = cake.luch == "1",
                    Description = cake.commo,
                    FillingId = fillingId
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""Cakes_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Cakes""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} cakes.", count);
    }

    private async Task MigrateTestimonialsAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating testimonials...");

        var testimonials = await mysqlConn.QueryAsync<MySqlTestimonial>(new CommandDefinition(@"
            SELECT id, data, email, name, text, otvet, razresh FROM guest",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var t in testimonials)
        {
            DateTime parsedDate;
            if (!DateTime.TryParseExact(t.data, "dd.MM.yyyy - HH:mm", null,
                System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                parsedDate = DateTime.UtcNow;
            }

            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""Testimonials"" (
                    ""Id"", ""Date"", ""Author"", ""Email"", 
                    ""Text"", ""Response"", ""IsApproved""
                )
                VALUES (@Id, @Date, @Author, @Email, @Text, @Response, @IsApproved)",
                new
                {
                    Id = t.id,
                    Date = parsedDate,
                    Author = t.name,
                    Email = string.IsNullOrEmpty(t.email) ? null : t.email,
                    Text = t.text,
                    Response = t.otvet,
                    IsApproved = t.razresh == "1"
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""Testimonials_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Testimonials""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} testimonials.", count);
    }

    private async Task MigratePagesAndBlocksAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating pages and content blocks...");

        var pages = await mysqlConn.QueryAsync<MySqlPage>(new CommandDefinition(@"
            SELECT id, name, name_rus, title, keywords, description, 
                   zagol, podzagol, content, klutch, tip, sorto 
            FROM great",
            cancellationToken: cancellationToken));

        var pageCount = 0;
        var pageIdMapping = new Dictionary<int, int>(); 

        foreach (var page in pages)
        {
            var sortOrder = int.TryParse(page.sorto, out var sort) ? sort : 0;

            var newId = await postgresConn.QuerySingleAsync<int>(new CommandDefinition(@"
                INSERT INTO ""Pages"" (
                    ""Slug"", ""Title"", ""MetaDescription"", 
                    ""Heading"", ""Subheading"", ""Content""
                )
                VALUES (@Slug, @Title, @MetaDescription, @Heading, @Subheading, @Content)
                RETURNING ""Id""",
                new
                {
                    Slug = page.name,
                    Title = page.title ?? "Eltorto",
                    MetaDescription = page.description ?? "Торты на заказ в Новосибирске",
                    Heading = page.zagol ?? page.name_rus,
                    Subheading = string.IsNullOrEmpty(page.podzagol) ? null : page.podzagol,
                    Content = string.IsNullOrEmpty(page.content) ? null : page.content
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            pageIdMapping[page.id] = newId;
            pageCount++;
        }

        _logger.LogInformation("Migrated {Count} pages.", pageCount);

        var blocks = await mysqlConn.QueryAsync<MySqlInfoBlock>(new CommandDefinition(@"
            SELECT id, kart, zagol, content, klutch, sorto FROM textblocks",
            cancellationToken: cancellationToken));

        var blockCount = 0;
        foreach (var block in blocks)
        {
            int? pageId = null;

            if (int.TryParse(block.klutch, out var oldPageId) && pageIdMapping.ContainsKey(oldPageId))
            {
                pageId = pageIdMapping[oldPageId];
            }
            else
            {
                _logger.LogWarning("Could not find page for block {BlockId} with klutch {Klutch}", block.id, block.klutch);
                continue;
            }

            var sortOrder = int.TryParse(block.sorto, out var sort) ? sort : 0;

            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""ContentBlocks"" (
                    ""PageId"", ""Title"", ""Text"", ""ImageUrl"", ""SortOrder""
                )
                VALUES (@PageId, @Title, @Text, @ImageUrl, @SortOrder)",
                new
                {
                    PageId = pageId,
                    Title = block.zagol ?? "Информационный блок",
                    Text = block.content,
                    ImageUrl = block.kart,
                    SortOrder = sortOrder
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            blockCount++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""Pages_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Pages""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} content blocks.", blockCount);
    }

    private async Task MigrateSliderAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating slider items...");

        var sliderItems = await mysqlConn.QueryAsync<MySqlSlider>(new CommandDefinition(@"
            SELECT id, kart, zagol, content, sorti FROM slider",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var item in sliderItems)
        {
            var sortOrder = int.TryParse(item.sorti, out var sort) ? sort : 0;

            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""SliderItems"" (""Id"", ""ImageUrl"", ""Title"", ""Subtitle"", ""SortOrder"")
                VALUES (@Id, @ImageUrl, @Title, @Subtitle, @SortOrder)",
                new
                {
                    Id = item.id,
                    ImageUrl = item.kart,
                    Title = string.IsNullOrEmpty(item.zagol) ? null : item.zagol,
                    Subtitle = string.IsNullOrEmpty(item.content) ? null : item.content,
                    SortOrder = sortOrder
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""SliderItems_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""SliderItems""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} slider items.", count);
    }

    private async Task MigrateContactsAsync(MySqlConnection mysqlConn, NpgsqlConnection postgresConn,
        NpgsqlTransaction transaction, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migrating contact settings...");

        var contacts = await mysqlConn.QueryAsync<MySqlContact>(new CommandDefinition(@"
            SELECT id, telone, teltwo, adres, email, karta FROM contacts",
            cancellationToken: cancellationToken));

        var count = 0;
        foreach (var contact in contacts)
        {
            await postgresConn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO ""ContactSettings"" (
                    ""Id"", ""Phone"", ""AdditionalPhone"", 
                    ""Email"", ""Address"", ""MapUrl""
                )
                VALUES (@Id, @Phone, @AdditionalPhone, @Email, @Address, @MapUrl)",
                new
                {
                    Id = contact.id,
                    Phone = contact.telone,
                    AdditionalPhone = string.IsNullOrEmpty(contact.teltwo) ? null : contact.teltwo,
                    Email = contact.email,
                    Address = string.IsNullOrEmpty(contact.adres) ? null : contact.adres,
                    MapUrl = string.IsNullOrEmpty(contact.karta) ? null : contact.karta
                },
                transaction: transaction,
                cancellationToken: cancellationToken));

            count++;
        }

        await postgresConn.ExecuteAsync(new CommandDefinition(@"
            SELECT setval('""ContactSettings_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""ContactSettings""), 0));",
            transaction: transaction,
            cancellationToken: cancellationToken));

        _logger.LogInformation("Migrated {Count} contact settings.", count);
    }

    public async Task DownAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Rolling back data migration...");

        using var postgresConn = new NpgsqlConnection(_postgresConnectionString);
        await postgresConn.OpenAsync(cancellationToken);

        await using var transaction = await postgresConn.BeginTransactionAsync(cancellationToken);

        try
        {
            await postgresConn.ExecuteAsync("SET session_replication_role = 'replica';", transaction: transaction);

            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"ContentBlocks\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Orders\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Cakes\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Testimonials\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Fillings\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Categories\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"Pages\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"SliderItems\" RESTART IDENTITY CASCADE;", transaction: transaction);
            await postgresConn.ExecuteAsync("TRUNCATE TABLE \"ContactSettings\" RESTART IDENTITY CASCADE;", transaction: transaction);

            await postgresConn.ExecuteAsync("SET session_replication_role = 'origin';", transaction: transaction);

            await postgresConn.ExecuteAsync(@"
                DELETE FROM ""__MigrationsHistory"" WHERE ""MigrationName"" = @Name",
                new { Name },
                transaction: transaction);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Rollback completed!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed!");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}