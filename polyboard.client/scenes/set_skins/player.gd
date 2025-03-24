extends Node

var rotating := false
var last_mouse_position := Vector2()

@onready var model_parent = $"CharacterBody2D/SubViewportContainer/SubViewport" 
@onready var previous_button = $"HButtonContainer/PreviousButton"
@onready var next_button = $"HButtonContainer/NextButton"
@onready var confirm_button = $"ConfirmButton"  

# Lista prefabrykowanych modeli 3D
var skins = SkinManager.get_skins()

var current_skin_index = 0  
var current_model = null  
var player_id = 0  

func _ready():
	previous_button.pressed.connect(_on_previous_pressed)
	next_button.pressed.connect(_on_next_pressed)
	confirm_button.pressed.connect(_on_confirm_pressed)  
	
	update_skin()

func _input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		rotating = event.pressed
		last_mouse_position = event.position

	if event is InputEventMouseMotion and rotating and current_model:
		var delta = event.position - last_mouse_position
		current_model.rotate_y(-delta.x * 0.004)  
		var new_x_rotation = current_model.rotation.x - delta.y * 0.0009
		
		current_model.rotation.x = clamp(new_x_rotation, deg_to_rad(-20), deg_to_rad(20))
		last_mouse_position = event.position

func _on_previous_pressed():
	current_skin_index -= 1
	if current_skin_index < 0:
		current_skin_index = skins.size() - 1 
	update_skin()

func _on_next_pressed():
	current_skin_index += 1
	if current_skin_index >= skins.size():
		current_skin_index = 0  
	update_skin()

func update_skin():
	if current_model:
		current_model.queue_free()
	
	current_model = skins[current_skin_index].instantiate()
	
	model_parent.add_child(current_model)

func _on_confirm_pressed():
	GameData.set_player_skin(current_skin_index)
	
	# FigureheadLoader.selected_pawn = skins[current_skin_index].resource_path  # Zapisujemy ścieżkę jako string
	get_tree().change_scene_to_file("res://scenes/local_lobby/local_lobby.tscn")
