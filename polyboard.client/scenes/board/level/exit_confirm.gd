extends Panel

signal exit_confirm_menu

func on_exit_button_pressed():
	get_tree().change_scene_to_file("res://scenes/main.menu/main_menu.tscn")

func on_cancel_button_pressed():
	exit_confirm_menu.emit()
	set_process(false)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
