extends Node

var rotating := false
var last_mouse_position := Vector2()

@onready var model_parent = $"CharacterBody2D/SubViewportContainer/SubViewport"  # Kontener na model
@onready var previous_button = $"HButtonContainer/PreviousButton"
@onready var next_button = $"HButtonContainer/NextButton"
@onready var confirm_button = $"ConfirmButton"  # Dodaj przycisk potwierdzający wybór

# Lista prefabrykowanych modeli 3D
var skins = [
	preload("res://scenes/board/figurehead/pizza_pionek.tscn"),   
	preload("res://scenes/board/figurehead/xbox_controller_pionek.tscn"),
	preload("res://scenes/board/figurehead/czapka_absolwenta_pionek.tscn"),
	preload("res://scenes/board/figurehead/lampka_pionek.tscn"),
	preload("res://scenes/board/figurehead/mikroskop_pionek.tscn"),
	preload("res://scenes/board/figurehead/lego_pionek.tscn")
]

var current_skin_index = 0  # Domyślny indeks
var current_model = null  # Referencja do obecnego modelu
var player_id = 0  # Identyfikator gracza (np. 0 dla pierwszego gracza)

func _ready():
	# Podpinamy przyciski do funkcji zmiany skinów
	previous_button.pressed.connect(_on_previous_pressed)
	next_button.pressed.connect(_on_next_pressed)
	confirm_button.pressed.connect(_on_confirm_pressed)  # Podpinamy potwierdzenie
	
	# Ustawiamy początkowy model
	update_skin()

func _input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		rotating = event.pressed
		last_mouse_position = event.position

	if event is InputEventMouseMotion and rotating and current_model:
		var delta = event.position - last_mouse_position
		current_model.rotate_y(-delta.x * 0.004)  # Obrót wokół osi Y (lewo/prawo)
		# Pobieramy aktualny kąt obrotu wokół osi X
		var new_x_rotation = current_model.rotation.x - delta.y * 0.0009
		
		# Ograniczamy do zakresu -20° do +20° (w radianach to około -0.35 do 0.35)
		current_model.rotation.x = clamp(new_x_rotation, deg_to_rad(-20), deg_to_rad(20))
		last_mouse_position = event.position

func _on_previous_pressed():
	current_skin_index -= 1
	if current_skin_index < 0:
		current_skin_index = skins.size() - 1  # Przechodzimy na ostatni model
	update_skin()

func _on_next_pressed():
	current_skin_index += 1
	if current_skin_index >= skins.size():
		current_skin_index = 0  # Wracamy do pierwszego modelu
	update_skin()

func update_skin():
	# Usuwamy poprzedni model, jeśli istnieje
	if current_model:
		current_model.queue_free()
	
	# Tworzymy nowy model na podstawie wybranego skinu
	current_model = skins[current_skin_index].instantiate()
	
	# Dodajemy model do kontenera w SubViewport
	model_parent.add_child(current_model)

func _on_confirm_pressed():
	# Przekazujemy wybraną ścieżkę modelu do GameManager przez FigureheadLoader
	FigureheadLoader.selected_pawn = skins[current_skin_index].resource_path  # Zapisujemy ścieżkę jako string
	# Możesz przełączyć scenę lub powiadomić użytkownika
	get_tree().change_scene_to_file("res://scenes/lobby/lobby.tscn")
