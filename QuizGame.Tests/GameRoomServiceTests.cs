using Microsoft.Extensions.DependencyInjection;
using Moq;
using QuizGame.Api.Models;
using QuizGame.Api.Services;


namespace QuizGame.Tests;

public class GameRoomServiceTests
{
    private readonly GameRoomService _service;

    public GameRoomServiceTests()
    {
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        _service = new GameRoomService(mockScopeFactory.Object);
    }

    [Fact]
    public void CreateRoom_WithValidPlayer_SetsRoomName()
    {
        //arrange 
        var player = MakePlayer("Alice");

        //Act
        var room = _service.CreateRoom("My Room", player);

        //Assert
        Assert.Equal("My Room", room.RoomName);
    }


    [Fact]
    public void JoinRoom_ExistingRoom_AddsPlayer()
    {
        //Arrange ------
        //create a room first
        var player = MakePlayer("Bruno");
        var room = _service.CreateRoom("My Room", player);

        // create a second player and add him to the room

        var player2 = MakePlayer("Neguinha");

        // Act ------
        _service.JoinRoom(room.RoomId, player2);

        // Assert ----

        Assert.Equal(2, room.PlayersInThisRoom.Count);
    }


    //MethodName_Condition_ExpectedResult
    [Fact]
    public void JoinRoom_RoomDoesNotExist_ReturnsNull()
    {
        //Arrange---
        var player = MakePlayer("Mario");
        //Act ----
        var result = _service.JoinRoom("FAKEID", player);

        //Assert
        Assert.Null(result);
    }


    [Fact]
    public void LeaveRoom_OnlyOnePlayerInTheRoom_ReturnsPlayer()
    {
        //Arrange ---
        var player = MakePlayer("Mario");
        var room = _service.CreateRoom("RoomId", player);

        //Act -----
        var returnedPlayer = _service.LeaveRoom(room.RoomId, player.ConnectionID);

        //Assert
        Assert.NotNull(returnedPlayer);
        Assert.Equal("Mario", returnedPlayer.Name);
        Assert.Empty(room.PlayersInThisRoom);

    }

    [Fact]
    public void LeaveRoom_PlayerIsNotInTheRoom_ReturnsNull()
    {
        //Arrange ----
        var player = MakePlayer("Mario");
        var room = _service.CreateRoom("RoomId", player);

        var player2 = MakePlayer("Caio");

        //Act ----
        var result = _service.LeaveRoom(room.RoomId, player2.ConnectionID);


        // Assert
        Assert.Null(result);
    }


    [Fact]

    public void RecordAnswer_NotAllPlayersAnswered_ReturnsFalse()
    {
        //Arrange ----
        var player1 = MakePlayer("Mario");
        var player2 = MakePlayer("Bruno");

        var room = _service.CreateRoom("RoomId", player1);
        _service.JoinRoom(room.RoomId, player2);

        //Act ----
        var result = _service.RecordAnswer(room.RoomId, player1.Name, 0);

        //Assert
        Assert.False(result);

    }


    [Fact]
    public void RecordAnswer_AllPlayerAnswered_ReturnsTrue()
    {
        //Arrange ----
        var player1 = MakePlayer("Mario");
        var player2 = MakePlayer("Bruno");

        var room = _service.CreateRoom("RoomId", player1);
        _service.JoinRoom(room.RoomId, player2);

        ////Act ----
        _service.RecordAnswer(room.RoomId, player1.Name, 0);
        var result = _service.RecordAnswer(room.RoomId, player2.Name, 0);


        //Assert
        Assert.True(result);


    }


    [Fact]
    public void EvaluateRound_CorrectAnswer_IncreaseScore()
    {

        //Arrange ----
        var player = MakePlayer("Bruno");
        var room = _service.CreateRoom("RoomId", player);

        room.QuestionsInThisRoom.Add(MakeQuestion(1));

        room.RoomState = GameState.Playing;

        room.CurrentAnswers[player.Name] = 1;

        //Act ----
        _service.EvaluateRound(room.RoomId);

        //Assert ----
        Assert.Equal(1, room.Scores[player.Name]);
    }


    [Fact]
    public void NextQuestion_MoreQuestionsExist_ReturnsNextQuestion()
    {
        //arrange ----
        var player = MakePlayer("Bruno");
        var room = _service.CreateRoom("RoomId", player);

        room.QuestionsInThisRoom.Add(MakeQuestion(1));
        room.QuestionsInThisRoom.Add(MakeQuestion(2));

        room.RoomState = GameState.Playing;
        //act----
        var result = _service.NextQuestion(room.RoomId);

        //assert---
        Assert.NotNull(result);
        Assert.Equal(1, room.CurrentQuestionIndex);
        Assert.IsType<Question>(result);
    }
    
    [Fact]
    public void NextQuestion_NoMoreQuestions_ReturnsNull()
    {
//arrange ----
        var player = MakePlayer("Bruno");
        var room = _service.CreateRoom("RoomId", player);

        room.QuestionsInThisRoom.Add(MakeQuestion(1));
        

        room.RoomState = GameState.Playing;
        //act----
        var result = _service.NextQuestion(room.RoomId);

        //assert---
        Assert.Null(result);
       
    }


    ///These are the mocks

    private Player MakePlayer(string name) => new Player
    {
        ConnectionID = Guid.NewGuid().ToString(),
        Name = name,
        JoinedAt = DateTime.UtcNow
    };

    private Question MakeQuestion(int correctIndex) => new Question
    {
        Id = 1,
        Text = "Test question?",
        Options = new List<string> { "Option A", "Option B", "Option C" },
        CorrectOptionIndex = correctIndex,
        Category = Categories.Math
    };


}