namespace QuizGame.Api.Models;

/// <summary>
/// Represents a player connected to an active game room.
/// </summary>
public class Player
{
    /// <summary>
    /// The SignalR connection ID assigned to this player's session. Used to identify the player on disconnect.
    /// </summary>
    public string ConnectionID { get; set; } = "";

    /// <summary>
    /// The display name chosen by the player when joining the room.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The player's current score in the active game.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// The UTC timestamp of when the player joined the room.
    /// </summary>
    public DateTime JoinedAt { get; set; }
}
