using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eltorto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCakePriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "min_weight_kg",
                table: "Cakes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "Cakes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "min_weight_kg",
                table: "Cakes");

            migrationBuilder.DropColumn(
                name: "price",
                table: "Cakes");
        }
    }
}
