using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomInTableRequestDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "RequestItemDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomName",
                table: "RequestItemDetail",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "RequestItemDetail");

            migrationBuilder.DropColumn(
                name: "RoomName",
                table: "RequestItemDetail");
        }
    }
}
