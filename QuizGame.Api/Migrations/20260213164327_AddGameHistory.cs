using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuizGame.Api.Models;

#nullable disable

namespace QuizGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGameHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    PlayerResults = table.Column<List<PlayerResult>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistories", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Options",
                value: new List<string> { "Berlin", "Paris", "Rome", "Madrid" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Options",
                value: new List<string> { "Earth", "Venus", "Mars", "Jupiter" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Options",
                value: new List<string> { "3", "4", "5", "6" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Options",
                value: new List<string> { "Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                column: "Options",
                value: new List<string> { "Atlantic", "Indian", "Arctic", "Pacific" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 6,
                column: "Options",
                value: new List<string> { "5", "6", "7", "8" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 7,
                column: "Options",
                value: new List<string> { "25", "30", "35", "40" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Options",
                value: new List<string> { "10", "11", "12", "14" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Options",
                value: new List<string> { "54", "56", "58", "64" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Options",
                value: new List<string> { "3.12", "3.14", "3.16", "3.18" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 11,
                column: "Options",
                value: new List<string> { "Childs", "Children", "Childes", "Childern" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 12,
                column: "Options",
                value: new List<string> { "Sad", "Angry", "Joyful", "Tired" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 13,
                column: "Options",
                value: new List<string> { "Noun", "Verb", "Adjective", "Adverb" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 14,
                column: "Options",
                value: new List<string> { "Him went to the store.", "He went to the store.", "He goed to the store.", "Him goed to the store." });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 15,
                column: "Options",
                value: new List<string> { "Runned", "Ran", "Runed", "Running" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 16,
                column: "Options",
                value: new List<string> { "The method runs on a separate thread", "The method contains asynchronous operations", "The method is static", "The method returns void" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 17,
                column: "Options",
                value: new List<string> { "ILogger", "IServiceProvider", "IConfiguration", "IHostBuilder" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 18,
                column: "Options",
                value: new List<string> { "3000", "8080", "5000", "4200" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 19,
                column: "Options",
                value: new List<string> { "FirstOrDefault", "Single", "Find", "Where" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 20,
                column: "Options",
                value: new List<string> { "HttpContext", "DbContext", "ChangeTracker", "ModelBuilder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameHistories");

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1,
                column: "Options",
                value: new List<string> { "Berlin", "Paris", "Rome", "Madrid" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2,
                column: "Options",
                value: new List<string> { "Earth", "Venus", "Mars", "Jupiter" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3,
                column: "Options",
                value: new List<string> { "3", "4", "5", "6" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                column: "Options",
                value: new List<string> { "Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5,
                column: "Options",
                value: new List<string> { "Atlantic", "Indian", "Arctic", "Pacific" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 6,
                column: "Options",
                value: new List<string> { "5", "6", "7", "8" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 7,
                column: "Options",
                value: new List<string> { "25", "30", "35", "40" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 8,
                column: "Options",
                value: new List<string> { "10", "11", "12", "14" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 9,
                column: "Options",
                value: new List<string> { "54", "56", "58", "64" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 10,
                column: "Options",
                value: new List<string> { "3.12", "3.14", "3.16", "3.18" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 11,
                column: "Options",
                value: new List<string> { "Childs", "Children", "Childes", "Childern" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 12,
                column: "Options",
                value: new List<string> { "Sad", "Angry", "Joyful", "Tired" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 13,
                column: "Options",
                value: new List<string> { "Noun", "Verb", "Adjective", "Adverb" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 14,
                column: "Options",
                value: new List<string> { "Him went to the store.", "He went to the store.", "He goed to the store.", "Him goed to the store." });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 15,
                column: "Options",
                value: new List<string> { "Runned", "Ran", "Runed", "Running" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 16,
                column: "Options",
                value: new List<string> { "The method runs on a separate thread", "The method contains asynchronous operations", "The method is static", "The method returns void" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 17,
                column: "Options",
                value: new List<string> { "ILogger", "IServiceProvider", "IConfiguration", "IHostBuilder" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 18,
                column: "Options",
                value: new List<string> { "3000", "8080", "5000", "4200" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 19,
                column: "Options",
                value: new List<string> { "FirstOrDefault", "Single", "Find", "Where" });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 20,
                column: "Options",
                value: new List<string> { "HttpContext", "DbContext", "ChangeTracker", "ModelBuilder" });
        }
    }
}
