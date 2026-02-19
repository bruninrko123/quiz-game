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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
            .WithOrigins("http://localhost:5173") // <-- your frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var app = builder.Build();

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
