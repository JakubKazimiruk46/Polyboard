using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class Lobby : Control
{
	//Cale gui na razie takie jak bylo
	//Zhardcodowane
	private string lobbypass = "abc";
	private string lobbyIdString = "6f83ae42-dac4-4d06-8311-e46df2f15d3c";

	public class User
	{
		public string Id { get; set; }
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		public bool IsReady { get; set; }
	}

	private PackedScene _lobbyPersonScene;
	private List<User> _users = new List<User>();
	private Dictionary<User, Control> _userGuiMap = new Dictionary<User, Control>();

	private TextureButton _backButton;
	private Button _readyButton;
	private Button _startGameButton;

	public override void _Ready()
	{
		GD.Print("Lobby initialized.\n");

		_lobbyPersonScene = (PackedScene)GD.Load("res://scenes/lobby/lobby_person.tscn");

		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");
		_readyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ReadyButton");
		_startGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/StartButton");
		_backButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer2/BackButton");

		_readyButton.Pressed += OnReadyButtonPressed;
		_startGameButton.Pressed += OnStartButtonPressed;
		_backButton.Pressed += OnBackButtonPressed;

		HubConnectionService.OnLobbyDetailsReceived += UpdateLobbyUsers;

		if (Guid.TryParse(lobbyIdString, out Guid lobbyId))
		{
			GD.Print("Fetching lobby details...");
			var hubConnectionService = new HubConnectionService();
			hubConnectionService.StartConnection();
			hubConnectionService.GetLobbyDetails(lobbyId, lobbypass);
		}
		else
		{
			GD.Print("Invalid lobby ID.");
		}
	}

	private void UpdateLobbyUsers(List<User> users)
	{
		GD.Print("Received lobby user details.\n");

		_users = users;

		foreach (var user in _users)
		{
			GD.Print($"User in lobby: {user.Username}");
		}

		PopulateUserList();
	}

	private void PopulateUserList()
	{
		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");
		userListContainer.ClearChildren();

		foreach (var user in _users)
		{
			var lobbyPersonInstance = (Control)_lobbyPersonScene.Instantiate();

			var usernameLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/Label");
			usernameLabel.Text = user.Username;

			var statusLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
			statusLabel.Text = user.IsReady ? "READY" : "NOT READY";
			statusLabel.AddThemeColorOverride("font_color", user.IsReady ? Colors.Green : Colors.Red);

			var textureRect = lobbyPersonInstance.GetNode<TextureRect>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/TextureRect");
			textureRect.Visible = user.Username == "exampleUser1";

			userListContainer.AddChild(lobbyPersonInstance);

			_userGuiMap[user] = lobbyPersonInstance;
		}

		if (_users.Count > 0)
		{
			_readyButton.Text = _users[0].IsReady ? "Unready" : "Ready";
		}
	}

	private void OnReadyButtonPressed()
	{
		if (_users.Count == 0)
			return;

		var firstUser = _users[0];
		firstUser.IsReady = !firstUser.IsReady;

		if (_userGuiMap.TryGetValue(firstUser, out var lobbyPersonInstance))
		{
			var statusLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
			statusLabel.Text = firstUser.IsReady ? "READY" : "NOT READY";
			statusLabel.AddThemeColorOverride("font_color", firstUser.IsReady ? Colors.Green : Colors.Red);
		}

		_readyButton.Text = firstUser.IsReady ? "Unready" : "Ready";

	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/board/level/level.tscn");
	}

	private void OnBackButtonPressed()
	{
		var _previousScenePath = "res://scenes/join.game/join_game.tscn";
		if (!string.IsNullOrEmpty(_previousScenePath))
		{
			GetTree().ChangeSceneToFile(_previousScenePath);
		}
	}
}

public static class NodeExtensions
{
	public static void ClearChildren(this Node parent)
	{
		foreach (var child in parent.GetChildren())
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
	}
}
