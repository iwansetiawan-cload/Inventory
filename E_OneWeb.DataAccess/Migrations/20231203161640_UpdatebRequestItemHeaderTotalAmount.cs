using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatebRequestItemHeaderTotalAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EntryBy",
                table: "RequestItemHeader",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EntryDate",
                table: "RequestItemHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalAmount",
                table: "RequestItemHeader",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryBy",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "EntryDate",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "RequestItemHeader");
        }
    }
}
