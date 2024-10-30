class_name MainMenu
extends Control

@onready var new_game_button = $MarginContainer/HBoxContainer/VBoxContainer/new_game_button as Button
@onready var join_game_button = $MarginContainer/HBoxContainer/VBoxContainer/join_game_button as Button
@onready var settings_button = $MarginContainer/HBoxContainer/VBoxContainer/join_game_button as Button
@onready var exit_button = $MarginContainer/HBoxContainer/VBoxContainer/exit_button as Button
@onready var login_button = $MarginContainer/HBoxContainer/VBoxContainer/login_button as Button
@onready var login_menu = $login as Login
@onready var margin_container = $MarginContainer as MarginContainer

func _ready():
	handle_connecting_signals()
	
func on_new_game_pressed() -> void:
	pass
	
func on_login_pressed() -> void:
	margin_container.visible = false
	login_menu.set_process(true)
	login_menu.visible = true

func on_exit_pressed() -> void:
	get_tree().quit()

func on_exit_login_menu() -> void:
	margin_container.visible = true
	login_menu.visible = false

func handle_connecting_signals() -> void:
	new_game_button.button_down.connect(on_new_game_pressed)
	exit_button.button_down.connect(on_exit_pressed)
	login_button.button_down.connect(on_login_pressed)
	login_menu.exit_login_menu.connect(on_exit_login_menu)
