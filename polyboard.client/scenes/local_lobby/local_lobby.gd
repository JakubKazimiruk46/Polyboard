extends Control

@onready var players_container = $MarginContainer/MainVContainer/PlayersContainer
@onready var add_player_button = $MarginContainer/MainVContainer/PlayerCountButtonsConatiner/AddPlayerButton
@onready var remove_player_button = $MarginContainer/MainVContainer/PlayerCountButtonsConatiner/RemovePlayerButton
@onready var start_button = $MarginContainer/MainVContainer/GeneralButtonsContainer/StartButton
@onready var back_button = $MarginContainer/MainVContainer/HBoxContainer2/BackButton
@onready var background_button=$MarginContainer/MainVContainer/GeneralButtonsContainer/BackgroundButton

const PLAYER_SCENE = preload("res://scenes/local_lobby/local_lobby_player.tscn")

signal game_started

var max_players = GameData.MAX_PLAYERS
var min_players = GameData.MIN_PLAYERS

func _ready():
	await get_tree().process_frame  
	
	if not add_player_button or not remove_player_button:
		push_error("Nie znaleziono przycisków AddPlayerButton lub RemovePlayerButton!")
		return

	add_player_button.pressed.connect(_on_add_player_pressed)
	remove_player_button.pressed.connect(_on_remove_player_pressed)
	start_button.pressed.connect(_on_start_button_pressed)
	back_button.pressed.connect(_on_back_button_pressed)
	background_button.pressed.connect(_on_background_button_pressed)
	
	for i in range(GameData.get_player_count()):
		_add_player_from_game_data(GameData.players[i]["name"], GameData.players[i]["skin"], i)
	
	update_buttons()

func _on_add_player_pressed():
	if GameData.get_player_count() >= max_players:
		print("Nie można dodać więcej graczy — osiągnięto limit:", max_players)
		return

	_add_player()
	update_buttons()
	print("Aktualna liczba graczy: " + str(GameData.get_player_count()))

func _on_remove_player_pressed():
	if GameData.get_player_count() <= min_players:
		print("Nie można usunąć gracza — osiągnięto minimum:", min_players)
		return

	GameData.remove_player()

	if players_container.get_child_count() > GameData.get_player_count():
		var last_player = players_container.get_child(players_container.get_child_count() - 1)
		players_container.remove_child(last_player)
		last_player.queue_free()

	update_buttons()
	print("Aktualna liczba graczy: " + str(GameData.get_player_count()))

func _on_start_button_pressed():
	var level_path = "res://scenes/board/level/level.tscn"
	if ResourceLoader.exists(level_path):
		game_started.emit()
		get_tree().change_scene_to_file(level_path)
	else:
		print("Błąd: Scena levelu nie istnieje pod ścieżką: " + level_path)
		
		
func _on_background_button_pressed():
	var background_select_path="res://scenes/local_lobby/background_select.tscn"
	if ResourceLoader.exists(background_select_path):
		get_tree().change_scene_to_file(background_select_path)
	else:
		print("Błąd: Scena levelu nie istnieje pod ścieżką: " + background_select_path)
	
	
func _on_back_button_pressed():
	GameData.reset_data()
	
	var menu_path = "res://scenes/main.menu/main_menu.tscn"
	if ResourceLoader.exists(menu_path):
		get_tree().change_scene_to_file(menu_path)
	else:
		print("Błąd: Scena menu nie istnieje pod ścieżką: " + menu_path)

func _add_player():
	GameData.add_player()

	var new_id = GameData.get_last_player_id()
	var name = GameData.get_player_name_by_id(new_id)
	var skin = GameData.get_player_skin_id_by_id(new_id)

	_add_player_from_game_data(name, skin, new_id)

	
func _add_player_from_game_data(player_name: String, player_skin: int, player_id: int):
	var new_player = PLAYER_SCENE.instantiate()
	new_player.player_id = player_id
	new_player.player_name_label_text = player_name

	if new_player is Control:
		new_player.custom_minimum_size.y = 50

	players_container.add_child(new_player)

func update_buttons():
	add_player_button.disabled = GameData.get_player_count() >= max_players
	remove_player_button.disabled = GameData.get_player_count() <= min_players
