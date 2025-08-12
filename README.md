# PZ2024-GRA-REPO

## Overview

**PZ2024-GRA-REPO** is a multiplayer board game platform developed as a team project.  
It consists of a .NET 8 backend server and a Godot-based client, supporting real-time gameplay, achievements, and user management.

---

## Project Structure

- **PolyBoard.Server**  
  The backend server, built with .NET 8, handles game logic, user authentication, achievements, and real-time communication via SignalR.
- **polyboard.client**  
  The game client, built with Godot Engine (C#), provides the user interface, game board, player avatars, and handles user interactions.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Godot Engine 4.x (Mono version)](https://godotengine.org/download)
- [Docker](https://www.docker.com/) (optional, for running the server and database)

---

### Backend Setup (`PolyBoard.Server`)

1. **Clone the repository:**
    ```sh
    git clone https://github.com/yourusername/pz2024-gra-repo.git
    cd pz2024-gra-repo/PolyBoard.Server
    ```

2. **Run with Docker (recommended):**
    ```sh
    docker-compose up --build
    ```
    This will start the server and a PostgreSQL database.

3. **Or run locally:**
    ```sh
    dotnet build
    dotnet run --project PolyBoard.Server.Presentation
    ```

4. **Configuration:**
    - Edit `appsettings.json` in `PolyBoard.Server.Presentation` for server settings.
    - Database settings are in `docker-compose.yml` and `db-data/`.

---

### Client Setup (`polyboard.client`)

1. **Open in Godot:**
    - Launch Godot Engine (Mono version).
    - Open the `polyboard.client` folder as a project.

2. **Run the game:**
    - Press <kbd>F5</kbd> in Godot to run the client.
    - The client connects to the backend server for multiplayer features.

---

## Features

- Multiplayer board game with real-time updates
- User authentication and registration
- Achievements and progress tracking
- Multiple player avatars (3D models)
- Save/load game state
- Lobby and matchmaking system

---

## Folder Structure

```
PolyBoard.Server/         # .NET backend server
  ├─ Application/         # Application logic and DTOs
  ├─ Core/                # Core entities and business logic
  ├─ Infrastructure/      # Data access, repositories, migrations
  ├─ Presentation/        # API controllers, SignalR hubs, entry point
  ├─ db-data/             # PostgreSQL data and config
  └─ docker-compose.yml   # Docker setup

polyboard.client/         # Godot game client
  ├─ assets/              # Game assets (models, textures, music)
  ├─ scenes/              # Godot scenes (UI, game board, etc.)
  ├─ services/            # Client-side logic and networking
  ├─ DTO/                 # Data transfer objects
  ├─ test/                # Unit tests
  └─ PolyBoard.Client.csproj # C# project file
```

---

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

---

## License

This project is licensed under the MIT License.

---

## Authors

- Team PZ2024  
  Jakub Kazimiruk  
  Łukasz Kotowski  
  Łukasz Halicki  
  Dawid Davtyan  
  Jakub Krzewski  
  Kacper Sawicki  
  Jakub Kondrat  
  Patryk Zadykowicz  
---

## Acknowledgements

- [Godot Engine](https://godotengine.org/)
- [.NET](https://dotnet.microsoft.com/)
- [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [PostgreSQL](https://www.postgresql.org/)
