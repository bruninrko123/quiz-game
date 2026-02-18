using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGame.Api.Models;

/// <summary>
/// The available question categories for a game.
/// </summary>
public enum Categories
{
    Math,
    English,
    GeneralKnowledge,
    DotNetDevelopment
}

/// <summary>
/// Represents a quiz question stored in the database.
/// </summary>
public class Question
{
    /// <summary>
    /// The primary key for this question in the database.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The question text displayed to players.
    /// </summary>
    [Required]
    public string Text { get; set; } = "";

    /// <summary>
    /// The list of answer options displayed to players. Stored as JSONB in PostgreSQL.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<string> Options { get; set; } = new();

    /// <summary>
    /// The zero-based index of the correct answer in <see cref="Options"/>.
    /// </summary>
    public int CorrectOptionIndex { get; set; }

    /// <summary>
    /// The category this question belongs to (ex: Math, .NET development, English).
    /// </summary>
    public Categories Category { get; set; }
}
