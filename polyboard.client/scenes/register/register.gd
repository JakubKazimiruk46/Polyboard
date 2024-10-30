class_name Register
extends Control

@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/back_button as Button

signal exit_register_menu

func _ready():
	back_button.button_down.connect(on_back_button_pressed)
	set_process(false)
	
func on_back_button_pressed() -> void:
	exit_register_menu.emit()
	set_process(false)
