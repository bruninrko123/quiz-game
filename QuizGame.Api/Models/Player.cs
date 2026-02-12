namespace QuizGame.Api.Models;

public class Player
{
    public string ConnectionID { get; set; } = "";

    public string Name { get; set; } = "";

    public int Score { get; set; }

    public DateTime JoinedAt { get; set; }
}