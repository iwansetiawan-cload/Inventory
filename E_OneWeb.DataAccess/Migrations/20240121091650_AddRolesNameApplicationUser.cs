using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesNameApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RolesName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RolesName",
                table: "AspNetUsers");
        }
    }
}
