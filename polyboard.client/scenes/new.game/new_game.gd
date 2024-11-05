class_name NewGame
extends Control

@onready var public_checkbox = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/public_checkbox 
@onready var private_checkbox = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/private_checkbox 
@onready var password = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer/password
@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/back_button as Button

signal exit_newgame_menu

func _ready():
	back_button.button_down.connect(on_back_button_pressed)
	public_checkbox.button_down.connect(on_public_checkbox_pressed)
	private_checkbox.button_down.connect(on_private_checkbox_pressed)

func on_back_button_pressed() -> void:
	exit_newgame_menu.emit()
	set_process(false)
	
func on_private_checkbox_pressed() -> void:
	password.visible = true
	

func on_public_checkbox_pressed() -> void:
	password.visible = false
