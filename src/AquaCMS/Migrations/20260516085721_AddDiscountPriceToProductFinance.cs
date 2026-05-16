using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaCMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountPriceToProductFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "discount_price",
                table: "product_finances",
                type: "numeric(18,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_price",
                table: "product_finances");
        }
    }
}
