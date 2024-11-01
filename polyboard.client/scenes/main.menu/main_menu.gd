class_name MainMenu
extends Control

@onready var newgame_button = $MarginContainer/HBoxContainer/VBoxContainer/new_game_button as Button
@onready var join_game_button = $MarginContainer/HBoxContainer/VBoxContainer/join_game_button as Button
@onready var settings_button = $MarginContainer/HBoxContainer/VBoxContainer/settings_button as Button
@onready var exit_button = $MarginContainer/HBoxContainer/VBoxContainer/exit_button as Button
@onready var login_button = $MarginContainer/HBoxContainer/VBoxContainer/login_button as Button
@onready var login_menu = $login as Login
@onready var newgame_menu = $new_game as NewGame
@onready var joingame_menu = $join_game as JoinGame
@onready var settings_menu = $settings as Settings
@onready var margin_container = $MarginContainer as MarginContainer



func _ready():
	handle_connecting_signals()
	

func on_login_pressed() -> void:
	margin_container.visible = false
	login_menu.set_process(true)
	login_menu.visible = true
	
func on_newgame_pressed() -> void:
	margin_container.visible = false
	newgame_menu.set_process(true)
	newgame_menu.visible = true
	
func on_joingame_pressed() -> void:
	margin_container.visible = false
	joingame_menu.set_process(true)
	joingame_menu.visible = true

func on_settings_pressed() -> void:
	margin_container.visible = false
	settings_menu.set_process(true)
	settings_menu.visible = true

func on_exit_login_menu() -> void:
	margin_container.visible = true
	login_menu.visible = false

func on_exit_newgame_menu() -> void:
	margin_container.visible = true
	newgame_menu.visible = false
	
func on_exit_joingame_menu() -> void:
	margin_container.visible = true
	joingame_menu.visible = false
	
func on_exit_settings_menu() -> void:
	margin_container.visible = true
	settings_menu.visible = false
	
func on_exit_pressed() -> void:
	get_tree().quit()

func handle_connecting_signals() -> void:
	newgame_button.button_down.connect(on_newgame_pressed)
	newgame_menu.exit_newgame_menu.connect(on_exit_newgame_menu)
	login_button.button_down.connect(on_login_pressed)
	login_menu.exit_login_menu.connect(on_exit_login_menu)
	join_game_button.button_down.connect(on_joingame_pressed)
	joingame_menu.exit_joingame_menu.connect(on_exit_joingame_menu)
	settings_button.button_down.connect(on_settings_pressed)
	settings_menu.exit_settings_menu.connect(on_exit_settings_menu)
	exit_button.button_down.connect(on_exit_pressed)
