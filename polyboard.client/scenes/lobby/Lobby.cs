using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Services;
using DTO;

public partial class Lobby : Control
{
	public class User
	{
		public string Id { get; set; }
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		public bool IsReady { get; set; }
	}
	
	private PackedScene _lobbyPersonScene;
	private List<User> _users;

	private Dictionary<User, Control> _userGuiMap = new Dictionary<User, Control>();
	
	private Label _lobbyNameLabel;
	
	private Button _backButton;
	private Button _readyButton;
	private Button _startGameButton;
	
	public Guid LobbyId { get; set; }
	
	private HubConnectionService _hubService;

	public override void _Ready()
	{		
		var dataTransferService = DataTransferService.Instance;

		if (Guid.TryParse(dataTransferService.CurrentLobbyId, out Guid lobbyId))
		{
			LobbyId = lobbyId;
		}
		else
		{
			GD.PrintErr("Invalid GUID format for LobbyId");
		}

		GD.Print($"Lobby scene loaded with ID: {LobbyId}");
		
		_lobbyNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/LobbyNameLabel");

		_lobbyPersonScene = (PackedScene)GD.Load("res://scenes/lobby/lobby_person.tscn");

		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");

		_readyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ReadyButton");
		_readyButton.Pressed += OnReadyButtonPressed;
		
		_startGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/StartButton");
		_startGameButton.Pressed += OnStartButtonPressed;
		
		_backButton = GetNode<Button>("MarginContainer/VBoxContainer/VBoxContainer/BackButton");
		_backButton.Pressed += OnBackButtonPressed;
		
		_hubService = new HubConnectionService();
		_hubService.LobbyDetailsFetched += OnLobbyDetailsFetched;
		FetchLobbyDetails();
	}
	
	private void FetchLobbyDetails()
	{
		GD.Print($"Fetching lobby details");

		_hubService.FetchLobbyDetails(LobbyId);
	}
	
	private void OnLobbyDetailsFetched(LobbyDetailsDTO lobbyDetails)
	{
		GD.Print($"Received lobby details for lobby: {lobbyDetails.LobbyName}");
		_lobbyNameLabel.Text = lobbyDetails.LobbyName;
	}
	
	private void OnReadyButtonPressed()
	{
		var firstUser = _users[0];

		firstUser.IsReady = !firstUser.IsReady;

		var lobbyPersonInstance = _userGuiMap[firstUser];
		var statusLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
		statusLabel.Text = firstUser.IsReady ? "READY" : "NOT READY";
		statusLabel.AddThemeColorOverride("font_color", firstUser.IsReady ? Colors.Green : Colors.Red);

		_readyButton.Text = firstUser.IsReady ? "Unready" : "Ready";
	}
	
	private void OnStartButtonPressed()
	{
		if (!(_users.Where(u => u.IsReady == false).Any()))
			GetTree().ChangeSceneToFile("res://scenes/board/level/level.tscn");
	}
	
	private void OnBackButtonPressed()
	{
		var _previousScenePath = "res://scenes/join.game/join_game.tscn";
		
		if (!string.IsNullOrEmpty(_previousScenePath))
		{
			GetTree().ChangeSceneToFile(_previousScenePath);
		}
		else
		{
			GD.PrintErr("Previous scene path is not set!");
		}
	}
}	
