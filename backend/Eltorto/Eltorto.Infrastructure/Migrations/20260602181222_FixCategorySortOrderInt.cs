using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eltorto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCategorySortOrderInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Categories"" ALTER COLUMN ""SortOrder"" TYPE integer USING (""SortOrder""::integer);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Categories"" ALTER COLUMN ""SortOrder"" TYPE text USING (""SortOrder""::text);");
        }
    }
}
