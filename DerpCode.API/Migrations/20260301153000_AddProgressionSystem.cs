using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerpCode.API.Migrations;

public partial class AddProgressionSystem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserProgress",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                TotalXp = table.Column<int>(type: "integer", nullable: false),
                Level = table.Column<int>(type: "integer", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProgress", x => x.UserId);
                table.ForeignKey(
                    name: "FK_UserProgress_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserProblemProgress",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                ProblemId = table.Column<int>(type: "integer", nullable: false),
                BestXp = table.Column<int>(type: "integer", nullable: false),
                FirstXpAwardedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastAwardedCycleIndex = table.Column<int>(type: "integer", nullable: false),
                FirstSubmitAtCurrentCycle = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                SubmitAttemptsCurrentCycle = table.Column<int>(type: "integer", nullable: false),
                OpenedHintIndicesCurrentCycle = table.Column<string>(type: "jsonb", nullable: false),
                LastSolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProblemProgress", x => new { x.UserId, x.ProblemId });
                table.ForeignKey(
                    name: "FK_UserProblemProgress_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserProblemProgress_Problems_ProblemId",
                    column: x => x.ProblemId,
                    principalTable: "Problems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserProblemProgress_ProblemId",
            table: "UserProblemProgress",
            column: "ProblemId");

        migrationBuilder.CreateTable(
            name: "ExperienceEvents",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                XpDelta = table.Column<int>(type: "integer", nullable: false),
                IdempotencyKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Metadata = table.Column<string>(type: "jsonb", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExperienceEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExperienceEvents_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExperienceEvents_IdempotencyKey",
            table: "ExperienceEvents",
            column: "IdempotencyKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ExperienceEvents_UserId",
            table: "ExperienceEvents",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExperienceEvents");

        migrationBuilder.DropTable(
            name: "UserProblemProgress");

        migrationBuilder.DropTable(
            name: "UserProgress");
    }
}
