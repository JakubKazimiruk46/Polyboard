class_name LobbyList
extends Control

@onready var join_button = $PanelContainer/MarginContainer/HBoxContainer/JoinButton as Button
@onready var margin_container = $PanelContainer/MarginContainer as MarginContainer
@onready var label_privacy = $PanelContainer/MarginContainer/HBoxContainer/PrivacyLabel as Label
@onready var label_lobby_name = $PanelContainer/MarginContainer/HBoxContainer/NameLabel as Label
@onready var label_players = $PanelContainer/MarginContainer/HBoxContainer/PlayersLabel as Label

signal join_lobby_menu
func _ready():
	join_button.pressed.connect(on_button_join_pressed)

func on_button_join_pressed() -> void:
	join_lobby_menu.emit()
	set_process(false)
