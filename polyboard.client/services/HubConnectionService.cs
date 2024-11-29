using Godot;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

public partial class HubConnectionService : Node
{
	private static readonly HubConnectionService? _instance;
	public HubConnection Connection { get; private set; }
	public HubConnectionService(string hubLink = "ws://localhost:8081/lobby")
	{
		Connection = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithKeepAliveInterval(TimeSpan.FromSeconds(25))
			.WithUrl(hubLink)
			.Build();
		Connection.StartAsync();
	}


	public static HubConnectionService GetInsatnce(string hubLink = "ws://localhost:8081/lobby")
	{
		return _instance ?? new HubConnectionService(hubLink);
	}

	public async Task CreateLobby(string lobbyName, int maxPlayers, string password)
	{
		await Connection.InvokeAsync("CreateLobby", lobbyName, maxPlayers, password);
	}

	public async Task SetUserReady(Guid lobbyId, bool isReady)
	{
		await Connection.InvokeAsync("SetUserReady", lobbyId, isReady);
	}
}
