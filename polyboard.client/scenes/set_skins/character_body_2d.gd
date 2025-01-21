extends CharacterBody2D

var rotating := false
var last_mouse_position := Vector2()

@onready var model_3d = $"SubViewportContainer/SubViewport/pizza-pionek"

func _input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			rotating = event.pressed
			last_mouse_position = event.position

	if event is InputEventMouseMotion and rotating:
		var delta = event.position - last_mouse_position
		model_3d.rotate_y(-delta.x * 0.01)  # Obrót wokół osi Y
		last_mouse_position = event.position
