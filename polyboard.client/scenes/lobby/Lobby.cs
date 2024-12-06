using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

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
	private TextureButton _backButton;
	private Button _readyButton;
	
	private Button _startGameButton;
	private HTTPRequest _httpRequest;
	private HubConnectionService _hubService;

	public override void _Ready()
	{

		_hubService = new HubConnectionService();
		RegisterEventListener();

		_lobbyPersonScene = (PackedScene)GD.Load("res://scenes/lobby/lobby_person.tscn");

		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");

		_users = JsonConvert.DeserializeObject<List<User>>(jsonInput);

		_readyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ReadyButton");
		_readyButton.Pressed += OnReadyButtonPressed;
		
		_startGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/StartButton");
		_startGameButton.Pressed += OnStartButtonPressed;
		
		_backButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer2/BackButton");
		_backButton.Pressed += OnBackButtonPressed;

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

	private void RegisterEventListener(){
		_hubService.Connection.On<LobbyUserDTO>("UserJoinedLobby", OnUserJoinedLobby);
		_hubService.Connection.On<string>("UserLeft", OnUserLeft);
    	_hubService.Connection.On<string, bool>("UserReadyStatusChanged", OnUserReadyStatusChanged);
	}

	private void OnUserJoinedLobby(LobbyUserDTO newUser){
		GD.Print($"User joined: {newUser.Username}");
		AddUserToUI(newUser);
	}

	private void OnReciveLobbyDetails(){
		var lobbyData = JsonConvert.DeserializeObject<LobbyDetailsDTO>(lobbyDetails.ToString());
		UpdateUserListUI(lobbyData.ConnectedUsers);
	}

	private void UpdateUserListUI(List<LobbyUserDTO> users){
		print("Current Users: ");
		foreach(var user in users){
			print($"Username: {user.Username}, Ready: {user.IsReady}");
		}
		
	}

	private void AddUserToUI(LobbyUserDTO user){
		var userInstance = (Control)_lobbyPersonScene.Instantiate();

		var usernameLabel = userInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/Label");
		usernameLabel.Text = user.Username;

		var statusLabel = userInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
		statusLabel.Text = user.IsReady ? "READY" : "NOT READY";
		statusLabel.AddThemeColorOverride("font_color", user.IsReady ? Colors.Green : Colors.Red);

		var userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");
		userListContainer.AddChild(userInstance);

		_userGuiMap[user] = userInstance;
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
		if(_users.All(u => u.IsReady))
			GetTree().ChangeSceneToFile("res://scenes/board/level/level.tscn");
		else
			GD.Print("Not All users are ready!");
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
