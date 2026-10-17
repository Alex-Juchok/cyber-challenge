using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberChallenge.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    TargetNumber = table.Column<int>(type: "int", nullable: false),
                    ResultNumber = table.Column<int>(type: "int", nullable: false),
                    MovesCount = table.Column<int>(type: "int", nullable: false),
                    ElapsedSeconds = table.Column<int>(type: "int", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    PlayedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameAttempts_IsDeleted",
                table: "GameAttempts",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameAttempts");
        }
    }
}
