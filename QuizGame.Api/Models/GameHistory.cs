using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizGame.Api.Models;

public class GameHistory
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string RoomName { get; set; } = "";
    public Categories Category { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    public int TotalQuestions { get; set; }
    [Column(TypeName = "jsonb")]
    public List<PlayerResult> PlayerResults { get; set; } = new();
}

public class PlayerResult
{
    public string Name { get; set; } = "";
    public int Score { get; set; }
    public bool IsWinner{ get; set; }
}