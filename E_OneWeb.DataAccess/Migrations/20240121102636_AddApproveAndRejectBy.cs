using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddApproveAndRejectBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApproveBy",
                table: "RequestItemHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApproveDate",
                table: "RequestItemHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "RequestItemHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "RequestItemHeader",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproveBy",
                table: "RequestItemDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApproveDate",
                table: "RequestItemDetail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "RequestItemDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "RequestItemDetail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproveBy",
                table: "ItemService",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApproveDate",
                table: "ItemService",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "ItemService",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "ItemService",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproveBy",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "ApproveDate",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "RequestItemHeader");

            migrationBuilder.DropColumn(
                name: "ApproveBy",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "ApproveDate",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "ApproveBy",
                table: "ItemService");

            migrationBuilder.DropColumn(
                name: "ApproveDate",
                table: "ItemService");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "ItemService");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "ItemService");
        }
    }
}
