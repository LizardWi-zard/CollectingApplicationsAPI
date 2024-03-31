using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CollectingApplicationsAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Author = table.Column<Guid>(type: "uuid", nullable: false),
                    Activity = table.Column<string>(type: "varchar(30)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", nullable: true),
                    Outline = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Status = table.Column<string>(type: "varchar", nullable: true),
                    EditTime = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "Activities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Доклад, 35-45 минут", "Report" },
                    { 2, "Мастеркласс, 1-2 часа", "Masterclass" },
                    { 3, "Дискуссия / круглый стол, 40-50 минут", "Discussion" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Applications");
        }
    }
}
