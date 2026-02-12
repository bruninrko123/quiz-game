using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGame.Api.Models;


public enum Categories
{
    Math,
    English,
    GeneralKnowledge,
    DotNetDevelopment
}

public class Question
{
    [Key]  // Marks this as the primary key
    public int Id { get; set; }

    [Required] // NOT NULL in database
    public string Text { get; set; } = "";

    [Column(TypeName = "jsonb")]  // Store as JSON in PostgreSQL
    public List<string> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }

    public Categories Category { get; set; }

    
}


