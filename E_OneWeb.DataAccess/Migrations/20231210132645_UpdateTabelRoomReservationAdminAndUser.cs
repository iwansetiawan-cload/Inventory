using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTabelRoomReservationAdminAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RoomReservationUser",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "RoomReservationAdmin",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RoomReservationUser");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "RoomReservationAdmin");
        }
    }
}
