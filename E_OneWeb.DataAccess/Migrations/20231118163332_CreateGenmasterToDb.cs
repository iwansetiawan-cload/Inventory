using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_OneWeb.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateGenmasterToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GENMASTER",
                columns: table => new
                {
                    IDGEN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GENCODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GENNAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GENVALUE = table.Column<double>(type: "float", nullable: true),
                    GENFLAG = table.Column<int>(type: "int", nullable: false),
                    ENTRYBY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ENTRYDATE = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GENMASTER", x => x.IDGEN);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GENMASTER");
        }
    }
}
