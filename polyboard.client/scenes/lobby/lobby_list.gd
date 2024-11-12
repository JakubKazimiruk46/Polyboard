class_name LobbyList
extends Control

@onready var button_join = $PanelContainer/MarginContainer/HBoxContainer/button_join as Button
@onready var margin_container = $PanelContainer/MarginContainer as MarginContainer

signal join_lobby_menu
func _ready():
	button_join.pressed.connect(on_button_join_pressed)

func on_button_join_pressed() -> void:
	join_lobby_menu.emit()
	set_process(false)
