extends Control

@onready var players_container = $MarginContainer/MainVContainer/PlayersContainer
@onready var add_player_button = $MarginContainer/MainVContainer/PlayerCountButtonsConatiner/AddPlayerButton
@onready var remove_player_button = $MarginContainer/MainVContainer/PlayerCountButtonsConatiner/RemovePlayerButton
@onready var start_button = $MarginContainer/MainVContainer/GeneralButtonsContainer/StartButton
@onready var back_button = $MarginContainer/MainVContainer/HBoxContainer2/BackButton

const PLAYER_SCENE = preload("res://scenes/local_lobby/local_lobby_player.tscn")

var max_players = 4
var min_players = 2

func _ready():
	await get_tree().process_frame  

	if not add_player_button or not remove_player_button:
		push_error("Nie znaleziono przycisków AddPlayerButton lub RemovePlayerButton!")
		return

	add_player_button.pressed.connect(_on_add_player_pressed)
	remove_player_button.pressed.connect(_on_remove_player_pressed)
	start_button.pressed.connect(_on_start_button_pressed)
	back_button.pressed.connect(_on_back_button_pressed)
	
	for i in range(min_players):
		_add_player()

	update_buttons()

func _on_add_player_pressed():
	if players_container.get_child_count() < max_players:
		_add_player()
		update_buttons()

func _on_remove_player_pressed():
	if players_container.get_child_count() > min_players:
		var last_player = players_container.get_child(players_container.get_child_count() - 1)
		players_container.remove_child(last_player)
		last_player.queue_free()
		update_buttons()
		
func _on_start_button_pressed():
	var level_path = "res://scenes/board/level/level.tscn"
	if ResourceLoader.exists(level_path):
		get_tree().change_scene_to_file(level_path)
	else:
		print("Błąd: Scena levelu nie istnieje pod ścieżką: " + level_path)
		
func _on_back_button_pressed():
	var menu_path = "res://scenes/main.menu/main_menu.tscn"
	if ResourceLoader.exists(menu_path):
		get_tree().change_scene_to_file(menu_path)
	else:
		print("Błąd: Scena menu nie istnieje pod ścieżką: " + menu_path)

func _add_player():
	var new_player = PLAYER_SCENE.instantiate()
	new_player.name = "Player" + str(players_container.get_child_count() + 1)

	if new_player is Control:
		new_player.custom_minimum_size.y = 50

	players_container.add_child(new_player)

func update_buttons():
	add_player_button.disabled = players_container.get_child_count() >= max_players
	remove_player_button.disabled = players_container.get_child_count() <= min_players
