class_name Settings
extends Control

@onready var back_button = $MarginContainer/back_button as Button

signal exit_settings_menu

func _ready():
	back_button.button_down.connect(on_back_button_pressed)
	set_process(false)
	
func on_back_button_pressed() -> void:
	exit_settings_menu.emit()
	set_process(false)
