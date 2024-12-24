using Godot;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

public class LobbyDetailsDTO
{
    public Guid Id;
    public string lobbyName;
    public int status;
}

public partial class HubConnectionService : Node
{
    public static HubConnection Connection;
    private string? _token;
    private static Guid _curentLobbyId;

    public override void _Ready()
    {
        // Pobierz token po pełnym załadowaniu drzewa sceny.
        _token = GD.Load<Script>("res://scenes/authentication/authentication.gd").Get("token").AsString();

        // Inicjalizacja po pobraniu tokena.
        Connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithKeepAliveInterval(TimeSpan.FromSeconds(10))
            .WithUrl("ws://localhost:8081/lobby",
                options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                })
            .Build();
        
        Connection.On<LobbyDetailsDTO>("ReceiveLobbyDetails",
            lobbyDetails =>
            {
                _curentLobbyId = lobbyDetails.Id;
            });
    }

    public async void StartConnection()
    {
        try
        {
            await Connection.StartAsync();
            GD.Print("Connection started successfully.");
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error starting connection: ", ex.Message);
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, string password)
    {
        try
        {
            await Connection.InvokeAsync("CreateLobby", lobbyName, maxPlayers, password);
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error creating lobby: ", ex.Message);
        }
    }

    public async void SetUserReady(bool isReady)
    {
        if (_curentLobbyId.Equals(Guid.Empty))
            return;
        
        try
        {
            await Connection.InvokeAsync("SetUserReady", _curentLobbyId, isReady);
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error setting user ready: ", ex.Message);
        }
    }

    public async void LeaveLobby()
    {
        await Connection.InvokeAsync("LeaveLobby", _curentLobbyId.ToString());
        _curentLobbyId = Guid.Empty;
    }
}