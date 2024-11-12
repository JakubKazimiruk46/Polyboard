class_name JoinGame
extends Control

@onready var back_button = $MarginContainer/VBoxContainer/back_button as TextureButton
@onready var lobby_button = $MarginContainer/VBoxContainer/HBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer/button_join as Button
@onready var margin_container = $MarginContainer as MarginContainer
@onready var lobby_list = $Lobby as Lobby
signal exit_joingame_menu
signal join_lobby_menu

func _ready():
	handle_connecting_signals()
	set_process(false)

func on_back_button_pressed() -> void:
	exit_joingame_menu.emit()
	set_process(false)
	
func on_lobby_pressed() -> void:
	margin_container.visible = false
	lobby_list.set_process(true)
	lobby_list.visible = true

func on_exit_lobby_menu() -> void:
	margin_container.visible = true
	lobby_list.visible = false
	  

func handle_connecting_signals() -> void:
	back_button.pressed.connect(on_back_button_pressed)
	lobby_list.exit_lobby_menu_join.connect(on_exit_lobby_menu)
	lobby_button.button_down.connect(on_lobby_pressed)
	


	
	
