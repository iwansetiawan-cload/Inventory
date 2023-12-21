using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableRoomReservationAdmin_addColumnBookingEnddate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "RoomReservationAdmin",
                newName: "BookingStartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingEndDate",
                table: "RoomReservationAdmin",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingEndDate",
                table: "RoomReservationAdmin");

            migrationBuilder.RenameColumn(
                name: "BookingStartDate",
                table: "RoomReservationAdmin",
                newName: "BookingDate");
        }
    }
}
