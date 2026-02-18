namespace QuizGame.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using QuizGame.Api.Services;
using QuizGame.Api.Models;
using QuizGame.Api.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handles the real-time interactions among players using SignalR
/// </summary>
public class QuizHub : Hub
{

    private readonly GameRoomService _roomService;

    private QuizDbContext _dbContext;

    // Track which room each connection is in (static = shared across all hub instances)
    private static readonly Dictionary<string, string> _connectionToRoom = new();

    private readonly IHubContext<QuizHub> _hubContext;

    //constructor
    public QuizHub(GameRoomService roomService, QuizDbContext dbContext, IHubContext<QuizHub> hubContext)
    {

        _roomService = roomService;
        _dbContext = dbContext;
        _hubContext = hubContext;

    }


    /// <summary>
    /// Called by SignalR when a client disconnects. Remove the diconnected player from their room
    /// </summary>
    /// <param name="exception">The exception that caused the disconnection, or null if the player disconnected gracefully</param>
    /// <remarks>
    /// Finishes the round if all the other players have answered, calls: <see cref="ProcessRoundEnd"/>
    /// Cancels the timer and removes the room from memory if the room is empty after the player leaves
    /// </remarks>
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
                if (room?.PlayersInThisRoom.Count == 0)
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

    /// <summary>
/// Creates a new game room and adds the caller as the first player and host.
/// </summary>
/// <param name="roomName">The display name for the room.</param>
/// <param name="playerName">The name of the player creating the room.</param>
/// <remarks>
/// This method is called by SignalR.
/// Emits <c>RoomCreated(roomId, roomName)</c> and <c>PlayerList(PlayersInThisRoom)</c> to the caller.
/// </remarks>
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

    /// <summary>
    /// Allows a player to join an existing room using the roomId.
    /// </summary>
    /// <param name="roomId">The ID of the room the player wants to join</param>
    /// <param name="playerName">The name of the player, which he chose</param>
    /// <remarks>
    /// This method is called by SignalR
    /// Emits <c>PlayerJoined(playerName) to the group</c>, and <c>PlayersList(room.PlayersInThisRoom)</c> and <c> RoomJoined(room.RoomId, room.RoomName) to the caller</c> 
    /// Emits <c>Error</c> if the room is not found or another player has the same name in the room.
    /// </remarks>
    public async Task JoinRoom(string roomId, string playerName)
    {
        var player = new Player
        {
            ConnectionID = Context.ConnectionId,
            Name = playerName,
            JoinedAt = DateTime.UtcNow,

        };

        var existingRoom = _roomService.GetRoomById(roomId);
        if (existingRoom != null && existingRoom.PlayersInThisRoom.Any(p => p.Name == playerName))
        {
            await Clients.Caller.SendAsync("Error", "A player with that name is already in the room");
            return;
        }

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

    /// <summary>
    /// Allows the host to start the game.
    /// </summary>
    /// <param name="roomId">The Id of the room where the game will be started</param>
    /// <param name="category">The Category of the questions chosen (ex: Math, .NET development, English)</param>
    /// <remarks>
    /// This method is called by SignalR
    /// Emits <c>GameStarted</c>, <c>ReceiveQuestion(question.Id, question.Text, question.Options)</c> and <c>TimerStarted(15)</c> to the group (the room)
    /// Emits <c>Error</c> if the game fails to start
    /// </remarks>
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

    /// <summary>
    /// Get the categories of questions available (ex: Math, .NET development, English)
    /// </summary>
    /// <remarks>
    /// This method is called by signalR
    /// Emits <c>ReceiveCategories(categories)</c> to the caller
    /// </remarks>
    public async Task GetCategories()
    {
        var categories = await _dbContext.Questions
        .Select(q => q.Category)
        .Distinct()
        .ToListAsync();

        await Clients.Caller.SendAsync("ReceiveCategories", categories);
    }

    /// <summary>
    /// Allows a player to submit his/her answer
    /// </summary>
    /// <param name="roomId">The Id of the room the player is in</param>
    /// <param name="questionId">The Id of the question the player answered</param>
    /// <param name="selectedOptionIndex">The option the player selected as an answer</param>
    /// <param name="playerName">The name of the player who is submitting an answer</param>
    ///<remarks>
    /// If all players have answered, cancel the timer and calls <see cref="ProcessRoundEnd"/> to evaluate the round
    /// </remarks>
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



    /// <summary>
    /// Evaluates the current round and advances the game to the next question or ends the game.
    /// </summary>
    /// <param name="roomId">The ID of the room to process the round end for.</param>
    /// <remarks>
    /// Emits <c>RoundResults(correct/incorrect)</c> and <c>UpdateScores(scores)</c> to the group.
    /// If there are more questions, waits 5 seconds then emits <c>ReceiveQuestion(question information)</c> and starts the timer <c>TimerStarted(15)</c>.
    /// If all questions are exhausted, emits <c>GameOver(scores)</c>, saves the game history to the database and resets the game
    /// </remarks>
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

    /// <summary>
    /// Starts a 15-second fire-and-forget timer for the current question.
    /// </summary>
    /// <param name="roomId">The ID of the room the timer is running for.</param>
    /// <remarks>
    /// Runs in a background thread using <see cref="Task.Run"/>. Uses <see cref="IHubContext{THub}"/>
    /// to send messages since it runs outside the SignalR hub lifecycle.
    /// If the timer expires before all players answer, evaluates the round and advances the game.
    /// Cancelled silently via <see cref="CancellationTokenSource"/> if all players answer early.
    /// </remarks>
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

