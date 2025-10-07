using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thryft.Migrations
{
    /// <inheritdoc />
    public partial class updateColoursandSizestoEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Products",
                newName: "Sizes");

            migrationBuilder.RenameColumn(
                name: "Colour",
                table: "Products",
                newName: "Colours");

            migrationBuilder.AddColumn<int>(
                name: "SelectedColour",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SelectedSize",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedColour",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "Sizes",
                table: "Products",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "Colours",
                table: "Products",
                newName: "Colour");
        }
    }
}
