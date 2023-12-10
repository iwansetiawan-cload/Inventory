using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameTbRequestItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefNumber",
                table: "RequestItemHeader",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "RequestItemHeader",
                newName: "ReqNumber");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "RequestItemHeader",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "RequestItemHeader");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "RequestItemHeader",
                newName: "RefNumber");

            migrationBuilder.RenameColumn(
                name: "ReqNumber",
                table: "RequestItemHeader",
                newName: "Name");
        }
    }
}
