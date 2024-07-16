using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogSharp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedColumnToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "isDeleted", table: "Users");
        }
    }
}
