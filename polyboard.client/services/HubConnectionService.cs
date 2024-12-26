using Godot;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LobbyUserDTO
{
	public string Id { get; set; }
	public string Username { get; set; }
	public bool IsReady { get; set; }
}

public class LobbyDetailsDTO
{
	public Guid Id;
	public string LobbyName;
	public int Status;
	public List<LobbyUserDTO>? ConnectedUsers { get; set; }
}

public partial class HubConnectionService : Node
{
	public static HubConnection? Connection;
	public static string? Token;
	private static Guid? _currentLobbyId;
	public static event Action<List<Lobby.User>> OnLobbyDetailsReceived;

	public override void _Ready()
	{
		// Setup SignalR connection
		Connection = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithKeepAliveInterval(TimeSpan.FromSeconds(10))
			.WithUrl("ws://localhost:8081/lobby", options =>
			{
				options.AccessTokenProvider = () => Task.FromResult(Token);
			})
			.Build();
		//Powinno wypisywac info z lobby
		//I generalnie wypisuje, ale puste - zle przekazuje GUID lobbyID
		Connection.On<LobbyDetailsDTO>("ReceiveLobbyDetails", lobbyDetails =>
		{
			_currentLobbyId = lobbyDetails.Id;
			GD.Print($"Lobby ID: {lobbyDetails.Id}");
			GD.Print($"Lobby Name: {lobbyDetails.LobbyName}");
			GD.Print($"Lobby Status: {lobbyDetails.Status}");

			if (lobbyDetails.ConnectedUsers != null)
			{
				foreach (var user in lobbyDetails.ConnectedUsers)
				{
					GD.Print($"User: {user.Username}");
				}
			}
			else
			{
				GD.Print("No connected users.");
			}

			var users = new List<Lobby.User>();
			if (lobbyDetails.ConnectedUsers != null)
			{
				foreach (var user in lobbyDetails.ConnectedUsers)
				{
					users.Add(new Lobby.User
					{
						Id = user.Id,
						Username = user.Username,
						IsReady = user.IsReady
					});
				}
			}

			OnLobbyDetailsReceived?.Invoke(users);
		});
	}

	public async void GetLobbyDetails(Guid lobbyId, string? password = null)
	{
		try
		{
			GD.Print("Sending request to fetch lobby details...");
			await Connection.InvokeAsync("SendLobbyDetails", lobbyId, password);
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error fetching lobby details: ", ex.Message);
		}
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
	
	public async void StopConnection()
{
	if (Connection != null && Connection.State != HubConnectionState.Disconnected)
	{
		try
		{
			await Connection.StopAsync();
			GD.Print("Connection stopped successfully.");
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error stopping connection: ", ex.Message);
		}
	}
}

	public async void CreateLobby(string lobbyName, int maxPlayers = 6, string? password = null)
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
		if (_currentLobbyId == null)
		{
			GD.PrintErr("Lobby ID is null. Can't set ready status.");
			return;
		}

		try
		{
			await Connection.InvokeAsync("SetUserReady", _currentLobbyId, isReady);
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error setting user ready status: ", ex.Message);
		}
	}

	public async void LeaveLobby()
	{
		if (_currentLobbyId == null)
		{
			GD.PrintErr("Lobby ID is null. Can't leave the lobby.");
			return;
		}

		try
		{
			await Connection.InvokeAsync("LeaveLobby", _currentLobbyId.ToString());
			_currentLobbyId = null;
		}
		catch (Exception ex)
		{
			GD.PrintErr("Error leaving the lobby: ", ex.Message);
		}
	}

	public static async void UpdateToken(string newToken)
	{
		Token = newToken;
		GD.Print($"Updated Token: {Token}");

		if (Connection != null)
		{
			await Connection.StopAsync();
			Connection = new HubConnectionBuilder()
				.WithAutomaticReconnect()
				.WithKeepAliveInterval(TimeSpan.FromSeconds(10))
				.WithUrl("ws://localhost:8081/lobby", options =>
				{
					options.AccessTokenProvider = () => Task.FromResult(Token);
				})
				.Build();
		}
	}
}
