FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SpythereGamesServices/SpythereGamesServices.csproj", "SpythereGamesServices/"]
RUN dotnet restore "SpythereGamesServices/SpythereGamesServices.csproj"
COPY . .
WORKDIR "/src/SpythereGamesServices"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SpythereGamesServices.dll"]
