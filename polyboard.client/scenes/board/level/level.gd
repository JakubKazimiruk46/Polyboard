extends Node3D
@onready var menu = $Menu

func _ready() -> void:
	pass

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		if menu.visible == false:
			menu.visible = true
		else:
			menu.visible = false
