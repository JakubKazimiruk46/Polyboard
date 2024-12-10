using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

public partial class Lobby : Control
{
	private PackedScene _lobbyPersonScene;
	private List<User> _users = new List<User>();
	private Dictionary<User, Control> _userGuiMap = new Dictionary<User, Control>();
	private TextureButton _backButton;
	private Button _readyButton;
	private Button _startGameButton;
	private VBoxContainer _userListContainer;
	private HubConnectionService _hubService;

	public override async void _Ready()
	{
		_lobbyPersonScene = (PackedScene)GD.Load("res://scenes/lobby/lobby_person.tscn");
		_userListContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/UserList");

		_readyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ReadyButton");
		_readyButton.Pressed += OnReadyButtonPressed;

		_startGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/StartButton");
		_startGameButton.Pressed += OnStartButtonPressed;

		_backButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer2/BackButton");
		_backButton.Pressed += OnBackButtonPressed;

		//Łączenie z hubem
		_hubService = new HubConnectionService();
		await _hubService.Connection.StartAsync();

		_hubService.Connection.On<List<User>>("UpdateUserList", UpdateUserList);

		await _hubService.FetchLobbyDetails(Guid.NewGuid());
	}

	private void UpdateUserList(List<User> users)
	{
		_users = users;
		//Czyszczenie
		_userListContainer.Clear();
		_userGuiMap.Clear();

		foreach (var user in _users)
		{
			var lobbyPersonInstance = (Control)_lobbyPersonScene.Instantiate();

			var usernameLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/Label");
			usernameLabel.Text = JWTDecoder(user.Username); //Dekoduje JWT otrzymane od serwera
			//Opcjonalnie - po prostu pobierać username z serwera

			var statusLabel = lobbyPersonInstance.GetNode<Label>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer2/Status");
			statusLabel.Text = user.IsReady ? "READY" : "NOT READY";
			statusLabel.AddThemeColorOverride("font_color", user.IsReady ? Colors.Green : Colors.Red);

			var textureRect = lobbyPersonInstance.GetNode<TextureRect>("PanelContainer/MarginContainer/HBoxContainer/HBoxContainer/TextureRect");
			textureRect.Visible = user.Username == "exampleUser1";

			_userListContainer.AddChild(lobbyPersonInstance);

			_userGuiMap[user] = lobbyPersonInstance;
		}

		if (_users.Count > 0)
		{
			var firstUser = _users[0];
			_readyButton.Text = firstUser.IsReady ? "Unready" : "Ready";
		}
	}

	private async void OnReadyButtonPressed()
	{
		if (_users.Count > 0)
		{
			var firstUser = _users[0];
			firstUser.IsReady = !firstUser.IsReady;

			await _hubService.Connection.InvokeAsync("SetUserReady", Guid.NewGuid(), firstUser.IsReady);

			_readyButton.Text = firstUser.IsReady ? "Unready" : "Ready";
		}
	}

	private void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/board/level/level.tscn");
	}

	private void OnBackButtonPressed()
	{
		var previousScenePath = "res://scenes/join.game/join_game.tscn";

		if (!string.IsNullOrEmpty(previousScenePath))
		{
			GetTree().ChangeSceneToFile(previousScenePath);
		}
	}

	public class User
	{
		public string Id { get; set; }
		public string ConnectionId { get; set; }
		public string Username { get; set; }
		public bool IsReady { get; set; }
	}
}
