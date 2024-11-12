class_name Lobby
extends Control

@onready var back_button_join = $MarginContainer/VBoxContainer2/HBoxContainer/back_button as TextureButton
@onready var correct_button = $MarginContainer/VBoxContainer2/HBoxContainer/correct_button as TextureButton
@onready var incorrect_button = $MarginContainer/VBoxContainer2/HBoxContainer/incorrect_button as TextureButton
signal exit_lobby_menu_join

func _ready():
	back_button_join.button_down.connect(on_back_button_join_pressed)
	correct_button.button_down.connect(on_correct_button_pressed)
	incorrect_button.button_down.connect(on_incorrect_button_pressed)
	set_process(false)

func on_back_button_join_pressed() -> void:
	exit_lobby_menu_join.emit()
	set_process(false)
	
func on_incorrect_button_pressed() -> void:
	correct_button.visible = true
	incorrect_button.visible = false

func on_correct_button_pressed() -> void:
	correct_button.visible = false
	incorrect_button.visible = true


	
