using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eltorto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestimonialRatingAndHomeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnHomePage",
                table: "Testimonials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Testimonials",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnHomePage",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Testimonials");
        }
    }
}
