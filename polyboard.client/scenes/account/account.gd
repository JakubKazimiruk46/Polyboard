class_name Account
extends Control

@onready var back_button = $MarginContainer/back_button as Button
@onready var username = $ScrollContainer/VboxContainer/username as LineEdit
@onready var email = $ScrollContainer/VboxContainer/email as LineEdit
@onready var current_pass = $ScrollContainer/VboxContainer/current_password as LineEdit
@onready var new_pass = $ScrollContainer/VboxContainer/new_password as LineEdit
@onready var confirm_new_pass = $ScrollContainer/VboxContainer/confirm_new_password as LineEdit
@onready var save_button = $ScrollContainer/VBoxContainer/save_button as Button
@onready var http_request = $HTTPRequest as HTTPRequest

signal exit_account_menu()

func _ready():
	handle_connecting_signals()
	set_process(false)

func _on_save_pressed() -> void:
	if username.text.strip() != "":
		
	

func on_back_button_pressed() -> void:
	exit_account_menu.emit()
	set_process(false)
	
func handle_connecting_signals() -> void:
	back_button.pressed.connect(on_back_button_pressed)
	
