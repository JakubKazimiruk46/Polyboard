using Godot;
using System;
using Microsoft.AspNetCore.SignalR.Client;

public partial class HubConnectionService : Node
{
	public HubConnection Connection { get; set; }
	public HubConnectionService()
	{
		Connection = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithKeepAliveInterval(TimeSpan.FromSeconds(25))
			.WithUrl("ws://localhost:8081/lobby")
			.Build();
		Connection.StartAsync();
	}
	public void CreateLobby(string lobbyName, int maxPlayers, string password)
	{
		 Connection.InvokeAsync("CreateLobby", lobbyName, maxPlayers, password);
	}

	public void SetUserReady(Guid lobbyId, bool isReady)
	{
		 Connection.InvokeAsync("SetUserReady", lobbyId, isReady);
	}
	
	public async Task FetchLobbyDetails(Guid lobbyId, string? password = null){
		await Connection.InvokeAsync("SendLobbyDetails", lobbyId, password);
	}
}
