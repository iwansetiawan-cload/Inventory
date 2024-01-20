using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class renameTableApplicationuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "AspNetUsers",
                newName: "Prodi");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "AspNetUsers",
                newName: "NIM");

            migrationBuilder.RenameColumn(
                name: "CardNumber",
                table: "AspNetUsers",
                newName: "Fakultas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Prodi",
                table: "AspNetUsers",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "NIM",
                table: "AspNetUsers",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "Fakultas",
                table: "AspNetUsers",
                newName: "CardNumber");
        }
    }
}
