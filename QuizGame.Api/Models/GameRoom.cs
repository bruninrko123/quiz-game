namespace QuizGame.Api.Models;

/// <summary>
/// Represents an active game room held in memory. All data is lost if the server restarts.
/// </summary>
public class GameRoom
{
    /// <summary>
    /// The unique 6-character code used by players to join this room.
    /// </summary>
    public string RoomId { get; set; } = "";

    /// <summary>
    /// The display name of the room, chosen by the host when creating it.
    /// </summary>
    public string RoomName { get; set; } = "";

    /// <summary>
    /// The current state of the room (Waiting, Playing, or Finished).
    /// </summary>
    public GameState RoomState { get; set; } = GameState.Waiting;

    /// <summary>
    /// The index of the question currently being asked in the room. Selected from <see cref="QuestionsInThisRoom"/>.
    /// </summary>
    public int CurrentQuestionIndex { get; set; } = 0;

    /// <summary>
    /// The full list of questions loaded for this game, filtered by the selected <see cref="Category"/>.
    /// </summary>
    public List<Question> QuestionsInThisRoom = new();

    /// <summary>
    /// The players currently in this room.
    /// </summary>
    public List<Player> PlayersInThisRoom = new();

    /// <summary>
    /// The answers submitted for the current round, keyed by player name and mapped to the selected option index.
    /// </summary>
    public Dictionary<string, int> CurrentAnswers = new();

    /// <summary>
    /// The cumulative scores for each player across all rounds, keyed by player name.
    /// </summary>
    public Dictionary<string, int> Scores = new();

    /// <summary>
    /// The cancellation token source for the current question's 15-second timer.
    /// Cancelled when all players answer before the timer expires.
    /// </summary>
    public CancellationTokenSource? QuestionTimerCts { get; set; }

    /// <summary>
    /// Lock object used to prevent race conditions between the timer expiry
    /// and all-players-answered paths evaluating the same round twice.
    /// </summary>
    public readonly object RoundLock = new();

    /// <summary>
    /// Indicates whether the current round has already been evaluated.
    /// Used alongside <see cref="RoundLock"/> to ensure the round is only processed once.
    /// </summary>
    public bool RoundEvaluated { get; set; } = false;

    /// <summary>
    /// The question category selected by the host for this game.
    /// </summary>
    public Categories Category { get; set; }
}
