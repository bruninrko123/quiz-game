using Microsoft.EntityFrameworkCore;
using QuizGame.Api.Models;

namespace QuizGame.Api.Data;

public class QuizDbContext : DbContext
{
    // Constructor - receives connection settings from Program.cs
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
        // Constructor body is EMPTY - just passes options to base class
    }

    // DbSet goes HERE - outside the constructor, inside the class
    public DbSet<Question> Questions { get; set; }

    //Registering the GameHistory in the DbContext
    public DbSet<GameHistory> GameHistories { get; set; }

    // This method configures the database model
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Seed initial questions
        modelBuilder.Entity<Question>().HasData(
            new Question
            {
                Id = 1,
                Text = "What is the capital of France?",
                Options = new List<string> { "Berlin", "Paris", "Rome", "Madrid" },
                CorrectOptionIndex = 1,
                Category = Categories.GeneralKnowledge

            },
            new Question
            {
                Id = 2,
                Text = "Which planet is known as the Red Planet?",
                Options = new List<string> { "Earth", "Venus", "Mars", "Jupiter" },
                CorrectOptionIndex = 2,
                Category = Categories.GeneralKnowledge
            },
            new Question
            {
                Id = 3,
                Text = "What is 2 + 2?",
                Options = new List<string> { "3", "4", "5", "6" },
                CorrectOptionIndex = 1,
                Category = Categories.Math
            },
            new Question
            {
                Id = 4,
                Text = "Who wrote Romeo and Juliet?",
                Options = new List<string> { "Charles Dickens", "William Shakespeare", "Jane Austen", "Mark Twain" },
                CorrectOptionIndex = 1,
                Category = Categories.GeneralKnowledge
            },

new Question
{
    Id = 5,
    Text = "What is the largest ocean on Earth?",
    Options = new List<string> { "Atlantic", "Indian", "Arctic", "Pacific" },
    CorrectOptionIndex = 3,
    Category = Categories.GeneralKnowledge
},
new Question
{
    Id = 6,
    Text = "How many continents are there?",
    Options = new List<string> { "5", "6", "7", "8" },
    CorrectOptionIndex = 2,
    Category = Categories.GeneralKnowledge
},

// ============ MATH (already have 1, need 4 more) ============

// Existing: Id 3

new Question
{
    Id = 7,
    Text = "What is 15% of 200?",
    Options = new List<string> { "25", "30", "35", "40" },
    CorrectOptionIndex = 1,
    Category = Categories.Math
},
new Question
{
    Id = 8,
    Text = "What is the square root of 144?",
    Options = new List<string> { "10", "11", "12", "14" },
    CorrectOptionIndex = 2,
    Category = Categories.Math
},
new Question
{
    Id = 9,
    Text = "What is 7 × 8?",
    Options = new List<string> { "54", "56", "58", "64" },
    CorrectOptionIndex = 1,
    Category = Categories.Math
},
new Question
{
    Id = 10,
    Text = "What is the value of Pi rounded to two decimal places?",
    Options = new List<string> { "3.12", "3.14", "3.16", "3.18" },
    CorrectOptionIndex = 1,
    Category = Categories.Math
},

// ============ ENGLISH (need all 5) ============

new Question
{
    Id = 11,
    Text = "What is the plural of 'child'?",
    Options = new List<string> { "Childs", "Children", "Childes", "Childern" },
    CorrectOptionIndex = 1,
    Category = Categories.English
},
new Question
{
    Id = 12,
    Text = "Which word is a synonym for 'happy'?",
    Options = new List<string> { "Sad", "Angry", "Joyful", "Tired" },
    CorrectOptionIndex = 2,
    Category = Categories.English
},
new Question
{
    Id = 13,
    Text = "What type of word is 'quickly'?",
    Options = new List<string> { "Noun", "Verb", "Adjective", "Adverb" },
    CorrectOptionIndex = 3,
    Category = Categories.English
},
new Question
{
    Id = 14,
    Text = "Which sentence is grammatically correct?",
    Options = new List<string> { "Him went to the store.", "He went to the store.", "He goed to the store.", "Him goed to the store." },
    CorrectOptionIndex = 1,
    Category = Categories.English
},
new Question
{
    Id = 15,
    Text = "What is the past tense of 'run'?",
    Options = new List<string> { "Runned", "Ran", "Runed", "Running" },
    CorrectOptionIndex = 1,
    Category = Categories.English
},

// ============ .NET DEVELOPMENT (need all 5) ============

new Question
{
    Id = 16,
    Text = "What does the 'async' keyword indicate in C#?",
    Options = new List<string> { "The method runs on a separate thread", "The method contains asynchronous operations", "The method is static", "The method returns void" },
    CorrectOptionIndex = 1,
    Category = Categories.DotNetDevelopment
},
new Question
{
    Id = 17,
    Text = "Which interface is used for dependency injection in .NET?",
    Options = new List<string> { "ILogger", "IServiceProvider", "IConfiguration", "IHostBuilder" },
    CorrectOptionIndex = 1,
    Category = Categories.DotNetDevelopment
},
new Question
{
    Id = 18,
    Text = "What is the default port for an ASP.NET Core application in development?",
    Options = new List<string> { "3000", "8080", "5000", "4200" },
    CorrectOptionIndex = 2,
    Category = Categories.DotNetDevelopment
},
new Question
{
    Id = 19,
    Text = "Which LINQ method returns a single element or throws if none is found?",
    Options = new List<string> { "FirstOrDefault", "Single", "Find", "Where" },
    CorrectOptionIndex = 1,
    Category = Categories.DotNetDevelopment
},
new Question
{
    Id = 20,
    Text = "What does EF Core use to track changes to entities?",
    Options = new List<string> { "HttpContext", "DbContext", "ChangeTracker", "ModelBuilder" },
    CorrectOptionIndex = 2,
    Category = Categories.DotNetDevelopment
}
        );
    }

    //supress the warning because of List<String> 
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)
        );
    }
}
