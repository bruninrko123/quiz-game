FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["QuizGame.Api/QuizGame.Api.csproj", "QuizGame.Api/"]
RUN dotnet restore "QuizGame.Api/QuizGame.Api.csproj"
COPY QuizGame.Api/ QuizGame.Api/
RUN dotnet publish "QuizGame.Api/QuizGame.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QuizGame.Api.dll"]
