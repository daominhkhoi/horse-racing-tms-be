# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HorseRacingTournamentManagementSystem_0/HorseRacingTournamentManagementSystem_0.csproj", "HorseRacingTournamentManagementSystem_0/"]
RUN dotnet restore "./HorseRacingTournamentManagementSystem_0/HorseRacingTournamentManagementSystem_0.csproj"
COPY . .
WORKDIR "/src/HorseRacingTournamentManagementSystem_0"
RUN dotnet build "./HorseRacingTournamentManagementSystem_0.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HorseRacingTournamentManagementSystem_0.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HorseRacingTournamentManagementSystem_0.dll"]
