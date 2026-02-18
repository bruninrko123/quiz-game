using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGame.Api.Models;

/// <summary>
/// Represents a completed game's results being sent to the db when a game ends
/// </summary>
public class GameHistory
{
    /// <summary>
    /// The ID of one specific game history. Used as the primary key for the Game History table
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The name of the room (chosen by the host when the game started)
    /// </summary>

    [Required]
    public string RoomName { get; set; } = "";

    /// <summary>
    /// The category of questions chosen by the host (Math, .NET development, English, etc.)
    /// </summary>

    public Categories Category { get; set; }

    /// <summary>
    /// The Date and Time the game was played
    /// </summary>

    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The number of questions in that room
    /// </summary>

    public int TotalQuestions { get; set; }

    /// <summary>
    /// The list of the results of each player in the room, including their score and whether they won. Stored as JSONB in PostgreSQL.
    /// </summary>

    [Column(TypeName = "jsonb")]
    public List<PlayerResult> PlayerResults { get; set; } = new();
}

/// <summary>
/// Represents a single player's results
/// </summary>
public class PlayerResult
{
    public string Name { get; set; } = "";
    public int Score { get; set; }
    public bool IsWinner { get; set; }
}