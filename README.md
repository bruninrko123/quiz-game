# Quiz Game

A real-time multiplayer quiz game where players join rooms, compete across categories, and see results live — built with .NET and React.

## Tech Stack

**Backend:** ASP.NET Core, SignalR, Entity Framework Core, PostgreSQL

**Frontend:** React, TypeScript, Tailwind CSS, Vite

## Features

- **Room System** — Create or join rooms with a 6-character code
- **Live Multiplayer** — Real-time question/answer flow via SignalR WebSockets
- **Question Categories** — General Knowledge, Math, English, .NET Development
- **15-Second Timer** — Server-authoritative countdown per question; unanswered players are marked wrong
- **Live Scoring** — Scores update after each round with correct/incorrect feedback
- **Game History** — Completed games are saved to PostgreSQL with player scores and winners
- **Player Validation** — Prevents empty names and duplicate names in the same room
- **Disconnect Handling** — Game continues when a player disconnects mid-game

## Project Structure

```
QuizGame.Api/              # ASP.NET Core backend
├── Hubs/QuizHub.cs        # SignalR hub — real-time game communication
├── Services/              # Game logic (room management, scoring, timer)
├── Models/                # Domain models (GameRoom, Player, Question, GameHistory)
├── Data/                  # EF Core DbContext and migrations
└── Controllers/           # REST endpoints (game history)

quizgame-client/           # React frontend
└── src/App.tsx            # Main application component
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- [PostgreSQL](https://www.postgresql.org/download/)

## Getting Started

### 1. Database Setup

Create a PostgreSQL database and update the connection string in `QuizGame.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=quizgame;Username=your_user;Password=your_password"
  }
}
```

### 2. Backend

```bash
cd QuizGame.Api
dotnet ef database update
dotnet run
```

The API runs on `http://localhost:5270`.

### 3. Frontend

```bash
cd quizgame-client
npm install
npm run dev
```

The client runs on `http://localhost:5173`.

## How to Play

1. Open the app and enter your name
2. **Create a room** (you become the host) or **Join a room** with a code
3. The host selects a category and starts the game
4. Answer each question within 15 seconds
5. Scores update after every round — highest score wins
