class_name Account
extends Control

@onready var back_button = $MarginContainer/back_button as Button

signal exit_account_menu()

func _ready():
	handle_connecting_signals()
	set_process(false)

func on_back_button_pressed() -> void:
	exit_account_menu.emit()
	set_process(false)
	
func handle_connecting_signals() -> void:
	back_button.pressed.connect(on_back_button_pressed)
	
