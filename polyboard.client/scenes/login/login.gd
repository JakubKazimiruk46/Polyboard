class_name Login
extends Control

@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/back_button as Button
@onready var register_button = $MarginContainer/HBoxContainer/VBoxContainer/register_button as Button
@onready var register_menu = $register as Register
@onready var margin_container = $MarginContainer as MarginContainer

signal exit_login_menu

func _ready():
	handle_connecting_signals()
	set_process(false)
	
func on_register_pressed() -> void:
	margin_container.visible = false
	register_menu.set_process(true)
	register_menu.visible = true
	
func on_exit_register_menu() -> void:
	margin_container.visible = true
	register_menu.visible = false

func on_back_button_pressed() -> void:
	exit_login_menu.emit()
	set_process(false)
	
func handle_connecting_signals() -> void:
	register_button.button_down.connect(on_register_pressed)
	back_button.button_down.connect(on_back_button_pressed)
	register_menu.exit_register_menu.connect(on_exit_register_menu)
