using QuizGame.Api.Services;
using QuizGame.Api.Hubs;
using QuizGame.Api.Data;           
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

//Register database context

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var secondPort = uri.Port > 0 ? uri.Port : 5432;
    connectionString = $"Host={uri.Host};Port={secondPort};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";



}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); // Enable JSON support for List<string>
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseNpgsql(dataSource)
);



// 👉 Your service
builder.Services.AddSingleton<GameRoomService>();

// ⚡ Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
    "http://localhost:5173",
    "https://quiz-game-k7hm.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    db.Database.Migrate();
}

    // 🔁 Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<QuizHub>("/quizhub");

// ⚡ Enable CORS
app.UseCors();


app.Run();
