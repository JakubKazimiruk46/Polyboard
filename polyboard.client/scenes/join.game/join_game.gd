class_name JoinGame
extends Control

@onready var back_button = $MarginContainer/back_button as TextureButton

signal exit_joingame_menu

func _ready():
	back_button.button_down.connect(on_back_button_pressed)
	set_process(false)
	
func on_back_button_pressed() -> void:
	print("cos")
	exit_joingame_menu.emit()
	set_process(false)
