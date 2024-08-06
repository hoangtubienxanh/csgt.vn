using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VietnamTrafficPolice.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DistributedCache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(449)", maxLength: 449, nullable: false, collation: "ascii_bin")
                        .Annotation("MySql:CharSet", "ascii"),
                    AbsoluteExpiration = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: true),
                    ExpiresAtTime = table.Column<DateTime>(type: "datetime(6)", maxLength: 6, nullable: false),
                    SlidingExpirationInSeconds = table.Column<long>(type: "bigint(20)", nullable: true),
                    Value = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_uca1400_ai_ci");

            migrationBuilder.CreateIndex(
                name: "Index_ExpiresAtTime",
                table: "DistributedCache",
                column: "ExpiresAtTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedCache");
        }
    }
}
