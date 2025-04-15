extends Control

@onready var background1 = $VBoxContainer/HBoxContainer1/TextureButton
@onready var background2 = $VBoxContainer/HBoxContainer1/TextureButton2
@onready var background3 = $VBoxContainer/HBoxContainer1/TextureButton3
@onready var background4 = $VBoxContainer/HBoxContainer2/TextureButton
@onready var background5 = $VBoxContainer/HBoxContainer2/TextureButton2
@onready var background6 = $VBoxContainer/HBoxContainer2/TextureButton3

var choosen_background_path: String

func _ready():
	background1.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/politechnika.jpg"))
	background2.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/dachmiasto.jpg"))
	background3.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/las.jpg"))
	background4.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/noc.jpg"))
	background5.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/centrummiasta.jpg"))
	background6.pressed.connect(_on_background_pressed.bind("res://assets/images/game_background/step.jpg"))

func _on_background_pressed(path: String):
	choosen_background_path = path
	print("Wybrano tło: ", choosen_background_path)
	
	GameData.set_background(choosen_background_path)

	var local_lobby_path = "res://scenes/local_lobby/local_lobby.tscn"
	if ResourceLoader.exists(local_lobby_path):
		get_tree().change_scene_to_file(local_lobby_path)
	else:
		print("Błąd: Nie znaleziono sceny:", local_lobby_path)
