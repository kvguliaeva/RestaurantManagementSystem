using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagementSystem.Data.Migrations
{
    public partial class MakeCategoryIdRequiredInDish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
            name: "CategoryId",
            table: "Dishes",
            nullable: false,
            oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
            name: "CategoryId",
            table: "Dishes",
            nullable: true,
            oldNullable: false);
        }
    }
}
