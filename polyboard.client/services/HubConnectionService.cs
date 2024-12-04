using Godot;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using DTO;

namespace Services{
public partial class HubConnectionService : Node
{
	public HubConnection Connection { get; set; }
	public event Action<LobbyDetailsDTO> LobbyDetailsFetchedEvent;
	[Signal]
	public delegate void LobbyDetailsFetchedEventHandler(LobbyDetailsDTO lobbyDetails);

	public HubConnectionService()
	{
		Connection = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithKeepAliveInterval(TimeSpan.FromSeconds(25))
			.WithUrl("ws://localhost:8081/lobby")
			.Build();
		
	Connection.StartAsync().ContinueWith(task =>
	{
		if (task.IsCompletedSuccessfully)
		{
			Connection.On<LobbyDetailsDTO>("ReceiveLobbyDetails", lobbyDetails =>
			{
				GD.Print("Lobby Details received: ", lobbyDetails.LobbyName);
				LobbyDetailsFetchedEvent?.Invoke(lobbyDetails);
			});

			GD.Print("SignalR connection started successfully.");
		}
		else
		{
			GD.PrintErr($"SignalR connection failed: {task.Exception?.Message}");
		}
	});
	}
	public void CreateLobby(string lobbyName, int maxPlayers, string password)
	{
		 Connection.InvokeAsync("CreateLobby", lobbyName, maxPlayers, password);
	}

	public void SetUserReady(Guid lobbyId, bool isReady)
	{
		 Connection.InvokeAsync("SetUserReady", lobbyId, isReady);
	}
	
	public void JoinLobby(string lobbyId, string? password = null)
	{
		GD.Print($"Attempting to join lobby: {lobbyId}");
		Connection.InvokeAsync("JoinLobby", Guid.Parse(lobbyId), password).ContinueWith(task =>
		{
			if (task.IsCompletedSuccessfully)
			{
				GD.Print($"Successfully joined lobby: {lobbyId}");
			}
			else
			{
				GD.PrintErr($"Error joining lobby: {task.Exception?.Message}");
			}
		});
	}
	public void FetchLobbyDetails(Guid lobbyId)
	{	
	try
	{
		Connection.InvokeAsync("SendLobbyDetails", lobbyId, null);
		GD.Print($"sendlobbydetails wywolano");
	}
	catch (Exception ex)
	{
		GD.PrintErr($"Failed to fetch lobby details: {ex.Message}");
	}
}
}
}
