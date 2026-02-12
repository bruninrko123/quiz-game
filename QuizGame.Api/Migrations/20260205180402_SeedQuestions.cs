using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuizGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectOptionIndex", "Options", "Text" },
                values: new object[,]
                {
                    { 1, 1, new List<string> { "Berlin", "Paris", "Rome", "Madrid" }, "What is the capital of France?" },
                    { 2, 2, new List<string> { "Earth", "Venus", "Mars", "Jupiter" }, "Which planet is known as the Red Planet?" },
                    { 3, 1, new List<string> { "3", "4", "5", "6" }, "What is 2 + 2?" },
                    { 4, 1, new List<string> { "Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain" }, "Who wrote Romeo and Juliet?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
