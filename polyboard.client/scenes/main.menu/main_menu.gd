class_name MainMenu
extends Control

@onready var board_button = $MarginContainer/HBoxContainer/VBoxContainer/board_button as Button
@onready var logout_button = $MarginContainer/HBoxContainer/VBoxContainer/logout_button as Button
@onready var account_button = $MarginContainer/HBoxContainer/VBoxContainer/account_button as Button
@onready var newgame_button = $MarginContainer/HBoxContainer/VBoxContainer/new_game_button as Button
@onready var join_game_button = $MarginContainer/HBoxContainer/VBoxContainer/join_game_button as Button
@onready var local_game_button = $MarginContainer/HBoxContainer/VBoxContainer/local_game_button as Button
@onready var settings_button = $MarginContainer/HBoxContainer/VBoxContainer/settings_button as Button
@onready var exit_button = $MarginContainer/HBoxContainer/VBoxContainer/exit_button as Button
@onready var login_button = $MarginContainer/HBoxContainer/VBoxContainer/login_button as Button
@onready var account_menu = $account as Account
@onready var login_menu = $login as Login
@onready var newgame_menu = $new_game as NewGame
@onready var joingame_menu = $join_game as JoinGame
@onready var settings_menu = $settings as Settings
@onready var margin_container = $MarginContainer as MarginContainer
@onready var hover_sound = $MarginContainer/HBoxContainer/VBoxContainer/HoverSound as AudioStreamPlayer
@onready var click_sound = $MarginContainer/HBoxContainer/VBoxContainer/ClickSound as AudioStreamPlayer

func _ready():
	play_hover_sound()
	handle_connecting_signals()
	update_menu_visibility()
		

func play_hover_sound() -> void:
	hover_sound.play()

func update_menu_visibility():
	if Authentication.token == "":
		newgame_button.visible = false
		join_game_button.visible = false
		account_button.visible = false
		logout_button.visible = false
		local_game_button.visible = false
		login_button.visible = true

	else:
		local_game_button.visible = true
		account_button.visible = true
		login_button.visible = false
		logout_button.visible = true
		newgame_button.visible = true
		join_game_button.visible = true
		
func on_board_entered() -> void:
	play_hover_sound()
		
func on_board_pressed() -> void:
	click_sound.play()
	get_tree().change_scene_to_file("res://scenes/board/level/level.tscn")

func on_account_pressed() -> void:
	click_sound.play()
	margin_container.visible = false
	account_menu.set_process(true)
	account_menu.visible = true

func on_logout_pressed() -> void:
	click_sound.play()
	Authentication.token = ""
	margin_container.visible = false
	login_menu.set_process(true)
	login_menu.visible = true

func on_login_pressed() -> void:
	click_sound.play()
	margin_container.visible = false
	login_menu.set_process(true)
	login_menu.visible = true
	
func on_newgame_pressed() -> void:
	click_sound.play()
	margin_container.visible = false
	newgame_menu.set_process(true)
	newgame_menu.visible = true
	
func on_joingame_pressed() -> void:
	click_sound.play()
	margin_container.visible = false
	joingame_menu.set_process(true)
	joingame_menu.visible = true

func on_settings_pressed() -> void:
	click_sound.play()
	margin_container.visible = false
	settings_menu.set_process(true)
	settings_menu.visible = true

func on_exit_account_menu() -> void:
	click_sound.play()
	margin_container.visible = true
	account_menu.visible = false

func on_exit_login_menu() -> void:
	click_sound.play()
	margin_container.visible = true
	login_menu.visible = false
	if Authentication.token == "":
		newgame_button.visible = false
		join_game_button.visible = false
		account_button.visible = false
		logout_button.visible = false
		login_button.visible = true
	else:
		account_button.visible = true
		login_button.visible = false
		logout_button.visible = true
		newgame_button.visible = true
		join_game_button.visible = true

func on_exit_newgame_menu() -> void:
	click_sound.play()
	margin_container.visible = true
	newgame_menu.visible = false
	
func on_exit_joingame_menu() -> void:
	click_sound.play()
	margin_container.visible = true
	joingame_menu.visible = false
	
func on_exit_settings_menu() -> void:
	click_sound.play()
	margin_container.visible = true
	settings_menu.visible = false
	
func on_exit_pressed() -> void:
	click_sound.play()
	get_tree().quit()
	
func on_local_game_pressed() -> void:
	click_sound.play()
	get_tree().change_scene_to_file("res://scenes/local_lobby/local_lobby.gd")

func handle_connecting_signals() -> void:
	board_button.button_down.connect(on_board_pressed)
	account_button.button_down.connect(on_account_pressed)
	account_menu.exit_account_menu.connect(on_exit_account_menu)
	logout_button.button_down.connect(on_logout_pressed)
	newgame_button.button_down.connect(on_newgame_pressed)
	newgame_menu.exit_newgame_menu.connect(on_exit_newgame_menu)
	local_game_button.button_down.connect(on_local_game_pressed)
	login_button.button_down.connect(on_login_pressed)
	login_menu.exit_login_menu.connect(on_exit_login_menu)
	join_game_button.button_down.connect(on_joingame_pressed)
	joingame_menu.exit_joingame_menu.connect(on_exit_joingame_menu)
	settings_button.button_down.connect(on_settings_pressed)
	settings_menu.exit_settings_menu.connect(on_exit_settings_menu)
	exit_button.button_down.connect(on_exit_pressed)
	board_button.mouse_entered.connect(play_hover_sound)
	login_button.mouse_entered.connect(play_hover_sound)
	newgame_button.mouse_entered.connect(play_hover_sound)
	account_button.mouse_entered.connect(play_hover_sound)
	join_game_button.mouse_entered.connect(play_hover_sound)
	settings_button.mouse_entered.connect(play_hover_sound)
	exit_button.mouse_entered.connect(play_hover_sound)
	logout_button.mouse_entered.connect(play_hover_sound)
