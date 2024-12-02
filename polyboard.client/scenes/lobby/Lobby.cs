using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public partial class Lobby : Control
{
	private readonly string jsonInput = @"[
		{
			""id"": ""a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6"",
			""connectionId"": ""z9y8x7w6-v5u4-t3s2-r1q0-p9o8n7m6l5k4"",
			""username"": ""exampleUser1"",
			""isReady"": false
		},
		{
			""id"": ""f1g2h3i4-j5k6-l7m8-n9o0-p1q2r3s4t5u6"",
			""connectionId"": ""v4w5x6y7-z8a9-b1c2-d3e4-f5g6h7i8j9k0"",
			""username"": ""exampleUser2"",
			""isReady"": true
		},
		{
			""id"": ""m1n2o3p4-q5r6-s7t8-u9v0-w1x2y3z4a5b6"",
			""connectionId"": ""c4d5e6f7-g8h9-i1j2-k3l4-m5n6o7p8q9r0"",
			""username"": ""exampleUser3"",
			""isReady"": true
		},
		{
			""id"": ""q1r2s3t4-u5v6-w7x8-y9z0-a1b2c3d4e5f6"",
			""connectionId"": ""h4i5j6k7-l8m9-n1o2-p3q4-r5s6t7u8v9w0"",
			""username"": ""exampleUser4"",
			""isReady"": true
		}
	]";

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
	
	private Button _readyButton;
	private Button _startGameButton;
	
	public Guid LobbyId { get; set; }
	
	private HubConnectionService _hubService;

	public override void _Ready()
	{
		_hubService = GetTree().Root.GetNode<HubConnectionService>("services/HubConnectionService");
		_hubService.Connect(nameof(HubConnectionService.LobbyDetailsFetchedEventHandler), new Callable(this, nameof(OnLobbyDetailsFetched)));
		FetchLobbyDetails();
		
		_lobbyPersonScene = (PackedScene)GD.Load("res://scenes/lobby/lobby_person.tscn");

		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");

		_users = JsonConvert.DeserializeObject<List<User>>(jsonInput);

		_readyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ReadyButton");
		_readyButton.Pressed += OnReadyButtonPressed;
		
		_startGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/StartButton");
		_startGameButton.Pressed += OnStartButtonPressed;
		
		_lobbyNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer/LobbyNameLabel");
		_lobbyNameLabel = 
		
		foreach (var user in _users)
		{
			var lobbyPersonInstance = (Control)_lobbyPersonScene.Instantiate();

			var usernameLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/Label");
			usernameLabel.Text = user.Username;

			var statusLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
			statusLabel.Text = user.IsReady ? "READY" : "NOT READY";
			statusLabel.AddThemeColorOverride("font_color", user.IsReady ? Colors.Green : Colors.Red);

			var textureRect = lobbyPersonInstance.GetNode<TextureRect>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/TextureRect");
			textureRect.Visible = user.Username == "exampleUser1"; // Przykład dla "admina"

			userListContainer.AddChild(lobbyPersonInstance);

			_userGuiMap[user] = lobbyPersonInstance;
		}

		var firstUser = _users[0];
		_readyButton.Text = firstUser.IsReady ? "Unready" : "Ready";
	}
	
	private void FetchLobbyDetails()
	{
		_hubService.FetchLobbyDetails(LobbyId);
	}
	
	private void OnLobbyDetailsFetched(LobbyDetailsDTO lobbyDetails)
	{
		GD.Print($"Received lobby details for lobby: {lobbyDetails.LobbyName}");
		_lobbyNameLabel.Text = lobbyDetails.LobbyName;
		//PopulateUserList(lobbyDetails.ConnectedUsers);
	}
	
	/*private void PopulateUserList(List<LobbyUserDTO> users)
	{
		foreach (Node child in UserListContainer.GetChildren())
		{
			child.QueueFree();
		}
		foreach (var user in users)
		{
			var userEntry = CreateUserEntry(user);
			UserListContainer.AddChild(userEntry);
		}
	}*/
	
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
}	
