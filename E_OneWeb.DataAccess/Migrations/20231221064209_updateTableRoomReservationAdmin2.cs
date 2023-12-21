using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableRoomReservationAdmin2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudyName",
                table: "RoomReservationUser",
                newName: "Study");

            migrationBuilder.RenameColumn(
                name: "DosenName",
                table: "RoomReservationUser",
                newName: "Dosen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Study",
                table: "RoomReservationUser",
                newName: "StudyName");

            migrationBuilder.RenameColumn(
                name: "Dosen",
                table: "RoomReservationUser",
                newName: "DosenName");
        }
    }
}
