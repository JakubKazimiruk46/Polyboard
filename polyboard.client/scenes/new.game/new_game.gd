class_name NewGame
extends Control

@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/back_button as Button
@onready var public_checkbox = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/public_checkbox as CheckBox
@onready var private_checkbox = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/private_checkbox as CheckBox
@onready var password_input = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer/password as LineEdit
@onready var lobby_name_input = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer/lobby_name as LineEdit
@onready var create_button = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/create_button as Button
@onready var error_label = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/error_label as Label
@onready var margin_container = $MarginContainer
@onready var lobby = $Lobby


signal exit_newgame_menu

func _ready():
	back_button.pressed.connect(on_back_button_pressed)
	create_button.pressed.connect(on_create_button_pressed)
	public_checkbox.pressed.connect(on_public_checkbox_pressed)
	private_checkbox.pressed.connect(on_private_checkbox_pressed)

func on_back_button_pressed() -> void:
	HubConnectionService.LeaveLobby()
	exit_newgame_menu.emit()

func on_public_checkbox_pressed() -> void:
	password_input.visible = false
	private_checkbox.set_pressed(false)

func on_private_checkbox_pressed() -> void:
	password_input.visible = true
	public_checkbox.set_pressed(false)

func on_create_button_pressed() -> void:
	var lobby_name = lobby_name_input.text.strip_edges()
	var is_public = public_checkbox.is_pressed()
	var is_private = private_checkbox.is_pressed()
	var password = password_input.text.strip_edges() if is_private else null
	HubConnectionService.CreateLobby(lobby_name, 4, password)

	error_label.text = ""

	if lobby_name == "":
		error_label.text = "Lobby name cannot be empty."
		return

	if is_private and password == "":
		error_label.text = "Password cannot be empty for a private lobby."
		return

	var lobby_data = {
		"LobbyName": lobby_name,
		"IsPrivate": is_private,
		"Password": password
	}

	var json = JSON.new()
	var json_data = json.stringify(lobby_data, "\t")
	print("Lobby Data:", json_data)

	error_label.text = "Lobby created successfully!"
	margin_container.visible = false
	lobby.visible = true
