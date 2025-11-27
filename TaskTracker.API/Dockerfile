FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5128

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["TaskTracker.API/TaskTracker.API.csproj", "TaskTracker.API/"]
COPY ["TaskTracker.Infrastructure/TaskTracker.Infrastructure.csproj", "TaskTracker.Infrastructure/"]
COPY ["TaskTracker.Application/TaskTracker.Application.csproj", "TaskTracker.Application/"]
COPY ["TaskTracker.Domain/TaskTracker.Domain.csproj", "TaskTracker.Domain/"]
RUN dotnet restore "TaskTracker.API/TaskTracker.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TaskTracker.API"
RUN dotnet build "TaskTracker.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "TaskTracker.API.dll"]
