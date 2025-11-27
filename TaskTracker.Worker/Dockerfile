FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["TaskTracker.Worker/TaskTracker.Worker.csproj", "TaskTracker.Worker/"]
COPY ["TaskTracker.Infrastructure/TaskTracker.Infrastructure.csproj", "TaskTracker.Infrastructure/"]
COPY ["TaskTracker.Application/TaskTracker.Application.csproj", "TaskTracker.Application/"]
COPY ["TaskTracker.Domain/TaskTracker.Domain.csproj", "TaskTracker.Domain/"]
RUN dotnet restore "TaskTracker.Worker/TaskTracker.Worker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TaskTracker.Worker"
RUN dotnet build "TaskTracker.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskTracker.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.Worker.dll"]
