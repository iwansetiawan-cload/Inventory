using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTabelRequestItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "RequestItemHeader",
                newName: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "RequestItemDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RequestItemDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "RequestItemDetail",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "RequestItemDetail");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "RequestItemHeader",
                newName: "Description");
        }
    }
}
