namespace QuizGame.Api.Models;



public class GameRoom
{
    public string RoomId { get; set; } = "";
    public string RoomName { get; set; }= "";
    public GameState RoomState { get; set; }= GameState.Waiting;
    public int CurrentQuestionIndex { get; set; } = 0;
    public List<Question> QuestionsInThisRoom = new();
    public List<Player> PlayersInThisRoom = new();

    public Dictionary<string, int> CurrentAnswers = new();

    public Dictionary<string, int> Scores = new();
}