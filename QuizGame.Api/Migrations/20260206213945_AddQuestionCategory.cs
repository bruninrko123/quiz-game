using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuizGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Options" },
                values: new object[] { 2, new List<string> { "Berlin", "Paris", "Rome", "Madrid" } });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Options" },
                values: new object[] { 2, new List<string> { "Earth", "Venus", "Mars", "Jupiter" } });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Options" },
                values: new object[] { 0, new List<string> { "3", "4", "5", "6" } });

            migrationBuilder.UpdateData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Options" },
                values: new object[] { 2, new List<string> { "Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain" } });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "CorrectOptionIndex", "Options", "Text" },
                values: new object[,]
                {
                    { 5, 2, 3, new List<string> { "Atlantic", "Indian", "Arctic", "Pacific" }, "What is the largest ocean on Earth?" },
                    { 6, 2, 2, new List<string> { "5", "6", "7", "8" }, "How many continents are there?" },
                    { 7, 0, 1, new List<string> { "25", "30", "35", "40" }, "What is 15% of 200?" },
                    { 8, 0, 2, new List<string> { "10", "11", "12", "14" }, "What is the square root of 144?" },
                    { 9, 0, 1, new List<string> { "54", "56", "58", "64" }, "What is 7 × 8?" },
                    { 10, 0, 1, new List<string> { "3.12", "3.14", "3.16", "3.18" }, "What is the value of Pi rounded to two decimal places?" },
                    { 11, 1, 1, new List<string> { "Childs", "Children", "Childes", "Childern" }, "What is the plural of 'child'?" },
                    { 12, 1, 2, new List<string> { "Sad", "Angry", "Joyful", "Tired" }, "Which word is a synonym for 'happy'?" },
                    { 13, 1, 3, new List<string> { "Noun", "Verb", "Adjective", "Adverb" }, "What type of word is 'quickly'?" },
                    { 14, 1, 1, new List<string> { "Him went to the store.", "He went to the store.", "He goed to the store.", "Him goed to the store." }, "Which sentence is grammatically correct?" },
                    { 15, 1, 1, new List<string> { "Runned", "Ran", "Runed", "Running" }, "What is the past tense of 'run'?" },
                    { 16, 3, 1, new List<string> { "The method runs on a separate thread", "The method contains asynchronous operations", "The method is static", "The method returns void" }, "What does the 'async' keyword indicate in C#?" },
                    { 17, 3, 1, new List<string> { "ILogger", "IServiceProvider", "IConfiguration", "IHostBuilder" }, "Which interface is used for dependency injection in .NET?" },
                    { 18, 3, 2, new List<string> { "3000", "8080", "5000", "4200" }, "What is the default port for an ASP.NET Core application in development?" },
                    { 19, 3, 1, new List<string> { "FirstOrDefault", "Single", "Find", "Where" }, "Which LINQ method returns a single element or throws if none is found?" },
                    { 20, 3, 2, new List<string> { "HttpContext", "DbContext", "ChangeTracker", "ModelBuilder" }, "What does EF Core use to track changes to entities?" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Questions");

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
        }
    }
}
