import { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

import { type Question } from "./types";
import { quizHubConnection } from "./signalr/quizHub";

interface Player {
  connectionID: string;
  name: string;
  joinedAt: string;
}

function App() {
  const [question, setQuestion] = useState<Question | null>(null);
  const [playerName, setPlayerName] = useState("");
  const [players, setPlayers] = useState<Player[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [hasJoined, setHasJoined] = useState(false);
  const [roomId, setRoomId] = useState<string>(""); // Current room code
  const [roomName, setRoomName] = useState<string>(""); // Room name for creating
  const [joinCode, setJoinCode] = useState<string>(""); // Code to join a room
  const [inRoom, setInRoom] = useState<boolean>(false); // Are we in a room?
  const [isHost, setIsHost] = useState<boolean>(false); // Did we create the room?
  const [categories, setCategories] = useState<number[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
  const [gameOver, setGameOver] = useState<boolean>(false);
  const [roundResults, setRoundResults] = useState<Record<
    string,
    boolean
  > | null>(null);
  const [scores, setScores] = useState<Record<string, number>>({});
  const [selectedAnswer, setSelectedAnswer] = useState<number | null>(null);
  const [timeLeft, setTimeLeft] = useState<number>(0);

  //mapping of the category names
  const categoryNames: Record<number, string> = {
    0: "Math",
    1: "English",
    2: "General Knowledge",
    3: ".NET Development",
  };

  useEffect(() => {
    // Register event handlers before starting connection
    quizHubConnection.on("PlayerJoined", (name: string) => {
      console.log(`${name} joined the game.`);
      setPlayers((prev) => [
        ...prev,
        { connectionID: "", name, joinedAt: new Date().toISOString() },
      ]);
    });

    quizHubConnection.on("PlayerLeft", (name: string) => {
      console.log(`${name} left the game.`);
      setPlayers((prev) => prev.filter((p) => p.name !== name));
    });

    quizHubConnection.on("PlayerList", (playerList: Player[]) => {
      console.log("Received player list:", playerList);
      setPlayers(playerList);
    });

    quizHubConnection.on("RoomCreated", (roomId: string, roomName: string) => {
      console.log(`Room created: ${roomName} (${roomId})`);
      setRoomId(roomId);
      setRoomName(roomName);
      setIsHost(true);
      setInRoom(true);
    });

    // Joined a room successfully
    quizHubConnection.on("RoomJoined", (roomId: string, roomName: string) => {
      console.log(`Joined room: ${roomName} (${roomId})`);
      setRoomId(roomId);
      setInRoom(true);
    });

    // Game started - host clicked start
    quizHubConnection.on("GameStarted", () => {
      console.log("Game has started!");
      setHasJoined(true); // Reuse your existing state to show the game
    });

    quizHubConnection.on(
      "ReceiveQuestion",
      (questionId: number, questionText: string, options: string[]) => {
        console.log("New question received");
        setQuestion({ id: questionId, text: questionText, options: options });
        setRoundResults(null); // clear last round's results
        setSelectedAnswer(null);
      },
    );

    quizHubConnection.on("ReceiveCategories", (categories: number[]) => {
      console.log(`Categories: ${categories}`);
      setCategories(categories);
    });

    quizHubConnection.on("GameOver", (finalScores) => {
      console.log("Game over");
      setGameOver(true);
      setRoundResults(null);
      setScores(finalScores);
    });

    quizHubConnection.on("RoundResults", (roundResutlts) => {
      console.log(roundResutlts);
      setRoundResults(roundResutlts);
      setTimeLeft(0);
    });

    quizHubConnection.on("UpdateScores", (scores) => {
      console.log(scores);
      setScores(scores);
    });

    quizHubConnection.on("TimerStarted", (seconds: number) => {
      setTimeLeft(seconds);
    });

    // Error from server
    quizHubConnection.on("Error", (message: string) => {
      console.error("Server error:", message);
      alert(message); // Simple alert for now
    });

    quizHubConnection.onclose(() => {
      setIsConnected(false);
    });

    quizHubConnection.onreconnected(() => {
      setIsConnected(true);
    });

    // Only start if disconnected (prevents double-start in React StrictMode)
    if (quizHubConnection.state === signalR.HubConnectionState.Disconnected) {
      quizHubConnection
        .start()
        .then(() => {
          console.log("Connected to SignalR");
          setIsConnected(true);
        })
        .catch((err) => console.error("SignalR connection error:", err));
    }

   

    return () => {
      quizHubConnection.off("PlayerJoined");
      quizHubConnection.off("PlayerLeft");
      quizHubConnection.off("PlayerList");
      quizHubConnection.off("RoomCreated");
      quizHubConnection.off("RoomJoined");
      quizHubConnection.off("GameStarted");
      quizHubConnection.off("ReceiveQuestion");
      quizHubConnection.off("Error");
      quizHubConnection.off("ReceiveCategories");
      quizHubConnection.off("GameOver");
      quizHubConnection.off("RoundResults");
      quizHubConnection.off("UpdateScores");
      quizHubConnection.off("TimerStarted");

      // 👇 manually remove lifecycle callbacks
      quizHubConnection.onclose(() => {});
      quizHubConnection.onreconnected(() => {});
    };
  }, []);

   useEffect(() => {
     if (timeLeft <= 0) return;

     const interval = setInterval(() => {
       setTimeLeft((prev) => {
         if (prev <= 1) {
           clearInterval(interval);
           return 0;
         }
         return prev - 1;
       });
     }, 1000);

     return () => clearInterval(interval);
   }, [timeLeft]);

  //handle answer submission
  async function handleAnswer(selectedIndex: number) {
    if (!question) return;
    setSelectedAnswer(selectedIndex);

    try {
      await quizHubConnection.invoke(
        "SubmitAnswer",
        roomId,
        question.id,
        selectedIndex,
        playerName,
      );
    } catch (err) {
      console.log(err);
    }
  }

  async function handleCreateRoom() {
    if (!isConnected) return;
    if (!roomName.trim()) {
      alert("Please enter a room name");
      return;
    }
    try {
      await quizHubConnection.invoke("CreateRoom", roomName, playerName);
      await quizHubConnection.invoke("GetCategories");
    } catch (error) {
      console.error("Failed to create room", error);
    }
  }

  async function handleJoinRoom() {
    if (!isConnected) return;
    if (!joinCode.trim()) {
      alert("Please enter a room code");
      return;
    }

    try {
      await quizHubConnection.invoke(
        "JoinRoom",
        joinCode.toUpperCase(),
        playerName,
      );
    } catch (error) {
      console.error("Failed to join game.", error);
    }
  }

  async function handleStartGame() {
    if (!isConnected || !isHost) return;
    try {
      await quizHubConnection.invoke("StartGame", roomId, selectedCategory);
    } catch (error) {
      console.error("Failed to start game.", error);
    }
  }

  const winner =
    Object.entries(scores).length > 0
      ? Object.entries(scores).reduce((best, current) =>
          current[1] > best[1] ? current : best,
        )
      : null;

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Step 1: Enter your name */}
      {!inRoom && (
        <div className="flex flex-col items-center justify-center min-h-screen p-6">
          <div className="bg-gray-800 rounded-2xl shadow-lg p-8 w-full max-w-md space-y-6">
            <h2 className="text-3xl font-bold text-center">Quiz Game</h2>

            <div className="flex flex-col gap-2">
              <label className="text-lg text-gray-300">Your Name</label>
              <input
                value={playerName}
                onChange={(e) => setPlayerName(e.target.value)}
                className="p-2 rounded bg-gray-700 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
              />
            </div>

            <hr className="border-gray-700" />

            {/* Create Room */}
            <div className="space-y-3">
              <h3 className="text-xl font-semibold">Create a Room</h3>
              <input
                placeholder="Room name (e.g., Friday Quiz)"
                value={roomName}
                onChange={(e) => setRoomName(e.target.value)}
                className="w-full p-2 rounded bg-gray-700 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
              />
              <button
                onClick={handleCreateRoom}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded transition"
              >
                Create Room
              </button>
            </div>

            <hr className="border-gray-700" />

            {/* Join Room */}
            <div className="space-y-3">
              <h3 className="text-xl font-semibold">Join a Room</h3>
              <input
                placeholder="Enter room code"
                value={joinCode}
                onChange={(e) => setJoinCode(e.target.value)}
                className="w-full p-2 rounded bg-gray-700 border border-gray-600 text-white focus:outline-none focus:border-blue-500"
              />
              <button
                onClick={handleJoinRoom}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded transition"
              >
                Join Room
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Step 2: In the room lobby */}
      {inRoom && !hasJoined && (
        <div className="flex flex-col items-center justify-center min-h-screen p-6">
          <div className="bg-gray-800 rounded-2xl shadow-lg p-8 w-full max-w-md space-y-6">
            <h2 className="text-3xl font-bold text-center">Room: {roomId}</h2>
            <p className="text-gray-400 text-center">
              Share this code with friends!
            </p>

            <div>
              <h3 className="text-xl font-semibold mb-2">Players</h3>
              <ul className="space-y-1">
                {players.map((p, i) => (
                  <li
                    key={p.connectionID || i}
                    className="bg-gray-700 rounded px-3 py-2"
                  >
                    {p.name}
                  </li>
                ))}
              </ul>
            </div>

            <div>
              <h3 className="text-xl font-semibold mb-2">Category</h3>
              <div className="flex flex-wrap gap-2">
                {categories.map((c, i) => (
                  <button
                    key={i}
                    onClick={() => setSelectedCategory(c)}
                    className={`py-2 px-4 rounded transition ${
                      selectedCategory === c
                        ? "bg-blue-600 text-white"
                        : "bg-gray-700 hover:bg-gray-600 text-gray-300"
                    }`}
                  >
                    {categoryNames[c]}
                  </button>
                ))}
              </div>
            </div>

            {isHost && (
              <button
                onClick={handleStartGame}
                className="w-full bg-green-600 hover:bg-green-700 text-white font-bold py-3 px-4 rounded transition text-lg"
              >
                Start Game
              </button>
            )}
            {!isHost && (
              <p className="text-gray-400 text-center">
                Waiting for host to start...
              </p>
            )}
          </div>
        </div>
      )}

      {/* Step 3: Playing the game */}
      {hasJoined && question && !gameOver && !roundResults && (
        <div className="flex flex-col items-center justify-center min-h-screen p-6">
          <div className="bg-gray-800 rounded-2xl shadow-lg p-8 w-full max-w-lg space-y-4">
            <h2 className="text-2xl font-bold text-center">{question.text}</h2>

            <div className="text-center">
              <span className={`text-3xl font-bold ${timeLeft <= 5 ? 'text-red-400' : 'text-green-400'}`}>
                {timeLeft}s
              </span>
              <div className="w-full bg-gray-700 rounded-full h-2 mt-2">
                <div
                className={`h-2 rounded-full transition-all duration-1000 ${timeLeft <= 5 ? 'bg-red-500' : 'bg-green-500'}`}
                style={{width: `${(timeLeft / 15) * 100}%`}}
                />
              </div>
            </div>

            <div className="space-y-3 mt-6">
              {question.options.map((opt, i) => (
                <button
                  key={i}
                  onClick={() => handleAnswer(i)}
                  className={`w-full bg-gray-700 hover:bg-blue-600 text-white py-3 px-4 rounded-lg transition text-left text-lg ${
                    selectedAnswer === i
                      ? "bg-blue-600 ring-2 ring-blue-400 text-white"
                      : "bg-gray-700 hover:bg-blue-600 text-white"
                  }`}
                >
                  {opt}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Round Results */}
      {roundResults && (
        <div className="flex flex-col items-center justify-center min-h-screen p-6">
          <div className="bg-gray-800 rounded-2xl shadow-lg p-8 w-full max-w-md space-y-6">
            <h2 className="text-2xl font-bold text-center">Round Results</h2>
            <ul className="space-y-2">
              {Object.entries(roundResults).map(([name, correct]) => (
                <li
                  key={name}
                  className={`flex justify-between items-center rounded px-4 py-2 ${
                    correct
                      ? "bg-green-900/50 text-green-300"
                      : "bg-red-900/50 text-red-300"
                  }`}
                >
                  <span>{name}</span>
                  <span>{correct ? "Correct" : "Wrong"}</span>
                </li>
              ))}
            </ul>

            <h3 className="text-xl font-semibold">Scores</h3>
            <ul className="space-y-2">
              {Object.entries(scores).map(([name, score]) => (
                <li
                  key={name}
                  className="flex justify-between bg-gray-700 rounded px-4 py-2"
                >
                  <span>{name}</span>
                  <span className="font-bold">{score}</span>
                </li>
              ))}
            </ul>
            <p className="text-gray-400 text-center">
              Next question in 5 seconds...
            </p>
          </div>
        </div>
      )}

      {/* Game Over */}
      {gameOver && (
        <div className="flex flex-col items-center justify-center min-h-screen p-6">
          <div className="bg-gray-800 rounded-2xl shadow-lg p-8 w-full max-w-md space-y-6 text-center">
            <h2 className="text-4xl font-bold">
              {winner ? `${winner[0]} Wins!` : "No Winner"}
            </h2>
            {winner && (
              <p className="text-xl text-gray-400">{winner[1]} points</p>
            )}

            <h3 className="text-xl font-semibold">Final Scores</h3>
            <ul className="space-y-2">
              {Object.entries(scores)
                .sort(([, a], [, b]) => b - a)
                .map(([name, score], i) => (
                  <li
                    key={name}
                    className={`flex justify-between rounded px-4 py-2 ${
                      i === 0
                        ? "bg-yellow-900/50 text-yellow-300"
                        : "bg-gray-700"
                    }`}
                  >
                    <span>{i === 0 ? `${name}` : name}</span>
                    <span className="font-bold">{score}</span>
                  </li>
                ))}
            </ul>

            <div className="flex gap-3 mt-4">
              <button
                onClick={() => {
                  setGameOver(false);
                  setHasJoined(false);
                  setSelectedCategory(null);
                  setQuestion(null);
                  setScores({});
                }}
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded transition"
              >
                Play Again
              </button>

              <button
                onClick={() => {
                  setGameOver(false);
                  setHasJoined(false);
                  setSelectedCategory(null);
                  setQuestion(null);
                  setInRoom(false);
                  setIsHost(false);
                  setPlayers([]);
                  setScores({});
                }}
                className="flex-1 bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded transition"
              >
                Leave Game
              </button>
            </div>
          </div>
        </div>
      )}

      {!isConnected && (
        <div className="fixed bottom-4 left-1/2 -translate-x-1/2 bg-red-900/80 text-red-200 px-4 py-2 rounded-lg">
          Connecting to game server...
        </div>
      )}
    </div>
  );
}

export default App;
