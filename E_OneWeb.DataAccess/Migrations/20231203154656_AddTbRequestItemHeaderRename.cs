using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTbRequestItemHeaderRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequesterDate",
                table: "RequestItemHeader",
                newName: "RequestDate");

            migrationBuilder.RenameColumn(
                name: "Files",
                table: "RequestItemHeader",
                newName: "RefFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "RequestItemHeader",
                newName: "RequesterDate");

            migrationBuilder.RenameColumn(
                name: "RefFile",
                table: "RequestItemHeader",
                newName: "Files");
        }
    }
}
