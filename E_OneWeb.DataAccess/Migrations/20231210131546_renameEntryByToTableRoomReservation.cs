using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class renameEntryByToTableRoomReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EntryDate",
                table: "RoomReservationAdmin",
                newName: "Bookingate");

            migrationBuilder.RenameColumn(
                name: "EntryBy",
                table: "RoomReservationAdmin",
                newName: "BookingBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Bookingate",
                table: "RoomReservationAdmin",
                newName: "EntryDate");

            migrationBuilder.RenameColumn(
                name: "BookingBy",
                table: "RoomReservationAdmin",
                newName: "EntryBy");
        }
    }
}
