extends Node3D

@onready var menu = $Menu
@onready var players_node = $GameManager/Players
@onready var game_manager = $GameManager

func _ready():
	# Pobierz wybraną ścieżkę modelu z FigureheadLoader
	var selected_pawn_path = FigureheadLoader.selected_pawn
	
	if selected_pawn_path == "":
		print("Error: No pawn selected!")
		return
	
	# Dodaj nowego gracza
	add_player(selected_pawn_path)

func add_player(model_path: String):
	# Wczytaj scenę `CharacterBody` (główna scena gracza)
	var player_scene = preload("res://scenes/board/level/player.tscn")  # Ścieżka do bazy gracza
	var new_player = player_scene.instantiate()
	
	# Wczytaj model na podstawie ścieżki z FigureheadLoader
	var model_scene = ResourceLoader.load(model_path)  # Dynamiczne ładowanie zasobu
	if not model_scene:
		print("Error: Failed to load model from path:", model_path)
		return
	
	var model_instance = model_scene.instantiate()

	# Dodaj model do węzła `CharacterBody` nowego gracza
	new_player.add_child(model_instance)
	
	# Dodaj nowego gracza do `Players`
	players_node.add_child(new_player)
	
	# Debugowanie
	print("Added player with model:", model_path)
	



func _input(event):
	if event.is_action_pressed("ui_cancel"):
		menu.visible = not menu.visible
