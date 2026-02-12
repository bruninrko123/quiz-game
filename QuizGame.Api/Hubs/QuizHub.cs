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

    //constructor
    public QuizHub( GameRoomService roomService, QuizDbContext dbContext)
    {
        
        _roomService = roomService;
        _dbContext = dbContext;

    }

  

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomId))
        {
            var player = _roomService.LeaveRoom(roomId, Context.ConnectionId);


            if (player != null)
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", player.Name);
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
            await Clients.Group(roomId).SendAsync("GameStarted");
            await Clients.Group(roomId).SendAsync("ReceiveQuestion", question.Id, question.Text, question.Options);


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
            var RoundResults = _roomService.EvaluateRound(roomId);

            await Clients.Group(roomId).SendAsync("RoundResults", RoundResults);

            var room = _roomService.GetRoomById(roomId);

            await Clients.Group(roomId).SendAsync("UpdateScores", room?.Scores);

            var nextQuestion = _roomService.NextQuestion(roomId);


        if (nextQuestion != null)
        {
            await Task.Delay(5000); // wait 5 seconds before sending the next questions so that players have time to see the results
            await Clients.Group(roomId).SendAsync("ReceiveQuestion", nextQuestion.Id, nextQuestion.Text, nextQuestion.Options);
        }
        else
            {
                var finalScores = _roomService.GetRoomById(roomId)?.Scores;
            await Clients.Group(roomId).SendAsync("GameOver", finalScores);
            _roomService.ResetGame(roomId);
        }
        }

        

    }
}

