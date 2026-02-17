namespace QuizGame.Api.Services;

	
using Microsoft.AspNetCore.SignalR;
using QuizGame.Api.Models;
using QuizGame.Api.Hubs;
using QuizGame.Api.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Manages in-memory game rooms and core game logic, such as room creation,
/// player management, question flow, scoring, and game history saving. 
/// </summary>
public class GameRoomService
{
	public readonly Dictionary<string, GameRoom> _rooms = new();
	public readonly IServiceScopeFactory _scopeFactory;


	public GameRoomService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	private string GenerateRoomId()
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		var code = new char[6];

		for (int i = 0; i < code.Length; i++)
		{
			code[i] = chars[Random.Shared.Next(chars.Length)];
		}

		var roomCode = new string(code);

		return _rooms.ContainsKey(roomCode) ? GenerateRoomId() : roomCode;

	}

	/// <summary>
	/// Creates a new game room and adds the player who created it as the first participant
	/// </summary>
	/// <param name="roomName"> The name of the room being created </param>
	/// <param name="player">An object for the first player, who created the room</param>
	/// <returns> The created room </returns> 

	public GameRoom CreateRoom(string roomName, Player player)
	{
		var room = new GameRoom
		{
			RoomName = roomName,
			RoomId = GenerateRoomId(),
			RoomState = GameState.Waiting,
			PlayersInThisRoom = new List<Player>()

		};

		room.PlayersInThisRoom.Add(player);

		_rooms.Add(room.RoomId, room);

		return room;
	}

	/// <summary>
	/// Allows a player to join an existing room
	/// </summary>
	/// <param name="roomId">The ID of the room the player will join</param>
	/// <param name="player">The player who is joining the room</param>
	/// <returns>
	/// Null if the room is not found, or the room with the player added, if the room is found
	/// </returns>
	public GameRoom? JoinRoom(string roomId, Player player)
	{

		if (_rooms.TryGetValue(roomId, out var room))
		{
			room.PlayersInThisRoom.Add(player);
			return room;
		}
		return null;

	}

	/// <summary>
	/// Find a room by its ID
	/// </summary>
	/// <param name="roomId">The ID of the room being searched for</param>
	/// <returns>The room, or null if the room is not found</returns>
	public GameRoom? GetRoomById(string roomId)
	{

		if (_rooms.TryGetValue(roomId, out var room))
		{
			return room;
		}
		return null;

	}

	/// <summary>
	/// Allows a player to leave the room
	/// </summary>
	/// <param name="roomID">The ID of the room the player is leaving</param>
	/// <param name="ConnectionID">SignalR's connection ID to that room</param>
	/// <returns>The removed player, or null if the room or the player is not found</returns>
	public Player? LeaveRoom(string roomID, string ConnectionID)
	{

		if (_rooms.TryGetValue(roomID, out var room))
		{
			var player = room.PlayersInThisRoom.FirstOrDefault(p => p.ConnectionID == ConnectionID);

			if (player != null)

			{
				room.PlayersInThisRoom.Remove(player);
				return player;
			}
		}
		return null;
	}

	/// <summary>
	/// Starts the game by loading questions for the selected category and setting the GameState to Playing
	/// </summary>
	/// <param name="roomId">The ID of the current room</param>
	/// <param name="category">The category of questions chosen for the game, ex: Math, .NET development, English</param>
	/// <returns>True if the room is found and is not yet started, False if the room is not found or if the Game has already started</returns>
	public bool StartGame(string roomId, Categories category)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			if (room.RoomState == GameState.Waiting)
			{
				// Create a scope to access the database
				using var scope = _scopeFactory.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();

				// Load questions from database
				room.QuestionsInThisRoom = db.Questions.Where(q => q.Category == category).ToList();

				room.CurrentQuestionIndex = 0;
				room.RoomState = GameState.Playing;
				room.Category = category;
				return true;

			}
			;

		}
		return false;
	}



	public Question? GetCurrentQuestion(string roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			if (room.RoomState == GameState.Playing && room.CurrentQuestionIndex < room.QuestionsInThisRoom.Count)
			{

				return room.QuestionsInThisRoom[room.CurrentQuestionIndex];
			}
		}
		return null;
	}

	public Question? NextQuestion(string roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			room.CurrentQuestionIndex++;
			return GetCurrentQuestion(roomId);
		}
		return null;
	}

	public void ResetGame(string roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			room.RoomState = GameState.Waiting;

		}
	}

	public bool RecordAnswer(string roomId, string playerName, int selectedOptionIndex)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			room.CurrentAnswers[playerName] = selectedOptionIndex;


			if (room.CurrentAnswers.Count == room.PlayersInThisRoom.Count)
			{
				return true;
			}

		}
		return false;
	}

	public Dictionary<string, bool> EvaluateRound(string roomId)
	{
		var results = new Dictionary<string, bool>();
		if (_rooms.TryGetValue(roomId, out var room))
		{
			var currentQuestionIndex = room.CurrentQuestionIndex;

			var currentQuestionCorrectOption = room.QuestionsInThisRoom[currentQuestionIndex].CorrectOptionIndex;

			foreach (var answer in room.CurrentAnswers)
			{
				if (answer.Value == currentQuestionCorrectOption)
				{
					room.Scores[answer.Key] = room.Scores.GetValueOrDefault(answer.Key, 0) + 1;

				}

				results[answer.Key] = (answer.Value == currentQuestionCorrectOption);
			}

			//Mark players who didn't answer as wrong

			foreach (var player in room.PlayersInThisRoom)
			{
				if (!results.ContainsKey(player.Name))
				{
					results[player.Name] = false;
				}
			}

			room.CurrentAnswers.Clear();
			return results;
		}
		return results;
	}

	public void ResetRoundState(string roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			room.CurrentAnswers.Clear();
			room.RoundEvaluated = false;
			room.QuestionTimerCts?.Cancel();
			room.QuestionTimerCts?.Dispose();
			room.QuestionTimerCts = new CancellationTokenSource();
		}
	}

	public async Task SaveGameHistory(string roomId)
	{
		if (_rooms.TryGetValue(roomId, out var room))
		{
			var maxScore = room.Scores.Values.Any() ? room.Scores.Values.Max() : 0;

			var playerResults = room.PlayersInThisRoom.Select(p => new PlayerResult
			{
				Name = p.Name,
				Score = room.Scores.GetValueOrDefault(p.Name, 0),
				IsWinner = room.Scores.GetValueOrDefault(p.Name, 0) == maxScore && maxScore > 0
			}).ToList();

			var history = new GameHistory
			{
				RoomName = room.RoomName,
				Category = room.Category,
				PlayedAt = DateTime.UtcNow,
				TotalQuestions = room.QuestionsInThisRoom.Count,
				PlayerResults = playerResults,
			};

			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
			db.GameHistories.Add(history);
			await db.SaveChangesAsync();
		}
	}

}
