using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class renameEntryByToTableRoomReservation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Bookingate",
                table: "RoomReservationAdmin",
                newName: "BookingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "RoomReservationAdmin",
                newName: "Bookingate");
        }
    }
}
