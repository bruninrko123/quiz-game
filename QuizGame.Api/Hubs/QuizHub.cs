namespace QuizGame.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using QuizGame.Api.Services;
using QuizGame.Api.Models;
using QuizGame.Api.Data;
using Microsoft.EntityFrameworkCore;

public class QuizHub : Hub
{
   
    private readonly GameRoomService _roomService;

    private QuizDbContext _dbContext;

    // Track which room each connection is in (static = shared across all hub instances)
    private static readonly Dictionary<string, string> _connectionToRoom = new();

    private readonly IHubContext<QuizHub> _hubContext;

    //constructor
    public QuizHub( GameRoomService roomService, QuizDbContext dbContext, IHubContext<QuizHub> hubContext)
    {
        
        _roomService = roomService;
        _dbContext = dbContext;
        _hubContext = hubContext;

    }

  

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomId))
        {
            var player = _roomService.LeaveRoom(roomId, Context.ConnectionId);


            if (player != null)
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", player.Name);

                //Check if this disconnect means all remaining players have answered
                var room = _roomService.GetRoomById(roomId);
                if (room != null && room.RoomState == GameState.Playing)
                {
                    // Remove disconnected player's answer if they had one
                    room.CurrentAnswers.Remove(player.Name);

                    // If remaining players have all answered, evaluate immediately
                    if (room.PlayersInThisRoom.Count > 0 &&
                    room.CurrentAnswers.Count == room.PlayersInThisRoom.Count)
                    {
                        room.QuestionTimerCts?.Cancel();

                        bool shouldProcess = false;
                        lock (room.RoundLock)
                        {
                            if (!room.RoundEvaluated)
                            {
                                room.RoundEvaluated = true;
                                shouldProcess = true;
                            }
                        }
                    if (shouldProcess)
                    {
                        await ProcessRoundEnd(roomId);
                    }
                    }
                }
                
                // If room is now empty, clean up
                if(room?.PlayersInThisRoom.Count == 0)
                {
                    room.QuestionTimerCts?.Cancel();
                    room.QuestionTimerCts?.Dispose();
                    _roomService._rooms.Remove(roomId);
                }

            }

            _connectionToRoom.Remove(Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
            await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateRoom(string roomName, string playerName)
    {
        var player = new Player
        {
            ConnectionID = Context.ConnectionId,
            Name = playerName,
            JoinedAt = DateTime.UtcNow,

        };

        var room = _roomService.CreateRoom(roomName, player);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);

        _connectionToRoom[Context.ConnectionId] = room.RoomId;

        await Clients.Caller.SendAsync("RoomCreated", room.RoomId, room.RoomName);

        await Clients.Caller.SendAsync("PlayerList", room.PlayersInThisRoom);

    }

    public async Task JoinRoom(string roomId, string playerName)
    {
        var player = new Player
        {
            ConnectionID = Context.ConnectionId,
            Name = playerName,
            JoinedAt = DateTime.UtcNow,

        };

        var room = _roomService.JoinRoom(roomId, player);

        if (room == null)
        {
            await Clients.Caller.SendAsync("Error", "Room  not found");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        _connectionToRoom[Context.ConnectionId] = roomId;

        await Clients.Group(roomId).SendAsync("PlayerJoined", playerName);

        await Clients.Caller.SendAsync("PlayersList", room.PlayersInThisRoom);

        await Clients.Caller.SendAsync("RoomJoined", room.RoomId, room.RoomName);


    }

    public async Task StartGame(string roomId, Categories category)
    {
        var started = _roomService.StartGame(roomId, category);

        if (!started)
        {
            await Clients.Caller.SendAsync("Error", "Could not start game");
            return;
        }

        var question = _roomService.GetCurrentQuestion(roomId);

        if (question != null)
        {
            _roomService.ResetRoundState(roomId);
            await Clients.Group(roomId).SendAsync("GameStarted");
            await Clients.Group(roomId).SendAsync("ReceiveQuestion", question.Id, question.Text, question.Options);
            await Clients.Group(roomId).SendAsync("TimerStarted", 15);
            StartQuestionTimer(roomId);

        }
    }

    public async Task GetCategories()
    {
        var categories = await _dbContext.Questions
        .Select(q => q.Category)
        .Distinct()
        .ToListAsync();

        await Clients.Caller.SendAsync("ReceiveCategories", categories);
    }

    public async Task SubmitAnswer(string roomId, int questionId, int selectedOptionIndex, string playerName)
    {
        // get the current question
        var currentQuestion = _roomService.GetCurrentQuestion(roomId);

        if (currentQuestion == null) return;

        // check it al players have answered the question
        var AllPlayersAnswered = _roomService.RecordAnswer(roomId, playerName, selectedOptionIndex);

        if (AllPlayersAnswered)
        {
            var room = _roomService.GetRoomById(roomId);
            if (room == null) return;

            //Cancel the timer (everyone answered so you can stop it)
            room.QuestionTimerCts?.Cancel();

            lock (room.RoundLock)
            {
                if (room.RoundEvaluated) return;
                room.RoundEvaluated = true;
            }

            await ProcessRoundEnd(roomId);
        }

    }

    private async Task ProcessRoundEnd(string roomId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null) return;

        var roundResults = _roomService.EvaluateRound(roomId);

        await Clients.Group(roomId).SendAsync("RoundResults", roundResults);
        await Clients.Group(roomId).SendAsync("UpdateScores", room.Scores);

        var nextQuestion = _roomService.NextQuestion(roomId);

        if (nextQuestion != null)
        {
            await Task.Delay(5000);
            _roomService.ResetRoundState(roomId);
            await Clients.Group(roomId).SendAsync("ReceiveQuestion", nextQuestion.Id, nextQuestion.Text, nextQuestion.Options);
            await Clients.Group(roomId).SendAsync("TimerStarted", 15);
            StartQuestionTimer(roomId);
        }
        else
        {
            await Clients.Group(roomId).SendAsync("GameOver", room.Scores);
            await _roomService.SaveGameHistory(roomId);
            _roomService.ResetGame(roomId);
        }
    }
    
    private void StartQuestionTimer(string roomId)
    {
        var room = _roomService.GetRoomById(roomId);
        if (room == null) return;

        var cts = room.QuestionTimerCts;
        if (cts == null) return;

        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(15000, token);

                
                lock (room.RoundLock)
                {
                    if (room.RoundEvaluated) return;
                    room.RoundEvaluated = true;
                }

                var roundResults = _roomService.EvaluateRound(roomId);

                await _hubContext.Clients.Group(roomId).SendAsync("RoundResults", roundResults);
                await _hubContext.Clients.Group(roomId).SendAsync("UpdateScores", room.Scores);

                var nextQuestion = _roomService.NextQuestion(roomId);

                if (nextQuestion != null)
                {
                    await Task.Delay(5000);
                    _roomService.ResetRoundState(roomId);
                    await _hubContext.Clients.Group(roomId).SendAsync("ReceiveQuestion", nextQuestion.Id, nextQuestion.Text, nextQuestion.Options);
                    await _hubContext.Clients.Group(roomId).SendAsync("TimerStarted", 15);
                    StartQuestionTimer(roomId);
                }
                else
                {
                    await _hubContext.Clients.Group(roomId).SendAsync("GameOver", room.Scores);
                    await _roomService.SaveGameHistory(roomId);
                    _roomService.ResetGame(roomId);
                }
            }
            catch (TaskCanceledException)
            {
                // Normal — all players answered before timer expired
                //Meaning we should not have any errors displayed here
            }
        });
    }
}

