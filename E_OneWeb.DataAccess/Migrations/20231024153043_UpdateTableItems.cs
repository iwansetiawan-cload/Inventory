using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SellPrice",
                table: "Items",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "BuyPrice",
                table: "Items",
                newName: "Price");

            migrationBuilder.AddColumn<double>(
                name: "DepreciationExpense",
                table: "Items",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginOfGoods",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Percent",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Qty",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepreciationExpense",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OriginOfGoods",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Percent",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Qty",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Items",
                newName: "SellPrice");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Items",
                newName: "BuyPrice");
        }
    }
}
