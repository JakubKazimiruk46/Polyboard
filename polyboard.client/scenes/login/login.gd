extends Control

class_name Login

#zmienne z przycisków
@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/back_button as Button
@onready var register_button = $MarginContainer/HBoxContainer/VBoxContainer/register_button as Button
@onready var username_input = $MarginContainer/HBoxContainer/VBoxContainer/username_input as TextEdit
@onready var password_input = $MarginContainer/HBoxContainer/VBoxContainer/password_input as LineEdit
@onready var login_button = $MarginContainer/HBoxContainer/VBoxContainer/login_button as Button
@onready var error_label = $MarginContainer/HBoxContainer/VBoxContainer/error_label as Label
@onready var register_menu = $register as Register
@onready var margin_container = $MarginContainer as MarginContainer

@onready var http_request = $HTTPRequest as HTTPRequest

signal exit_login_menu

func _ready():
	http_request.connect("request_completed", _on_request_completed, 1)
	handle_connecting_signals()
	set_process(false)

func on_register_pressed() -> void:
	margin_container.visible = false
	register_menu.set_process(true)
	register_menu.visible = true

func on_login_pressed() -> void:
	error_label.text = ""

	var username = username_input.text.strip_edges()
	var password = password_input.text.strip_edges()

	if username == "":
		error_label.text = "Username cannot be empty."
		return
	if password == "":
		error_label.text = "Password cannot be empty."
		return
		
	var login_data = {
		"UserName": username,
		"Password": password
	}
		
	var json = JSON.new()
	var json_data = json.stringify(login_data)
	#TODO URL DO ZMIANY!
	var url = "http://localhost:5000/login"
	
	var headers = ["Content-Type: application/json"]
	var error = http_request.request(url, headers, HTTPClient.METHOD_POST, json_data)
	
	var json_data1 = json.stringify(login_data)
	print("JSON Data being sent:", json_data1)
	
	if error != OK:
		error_label.text = "Failed to send request."
		print("Error sending request: ", error)

func _on_request_completed(result: int, response_code: int, headers: Array, body: PackedByteArray):
	var response_text = body.get_string_from_utf8()
	print("Raw response text: ", response_text)
	
	if response_code != 200:
		error_label.text = "Failed to parse response."
		print("Error parsing response: ", response_text)
		return

	if response_code == 200:
		print("Login successful: ", response_text)
		error_label.text = "Login successful!"
		exit_login_menu.emit()
		set_process(false)
	else:
		error_label.text = "Login failed: " + response_text

func on_exit_register_menu() -> void:
	margin_container.visible = true
	register_menu.visible = false

func on_back_button_pressed() -> void:
	exit_login_menu.emit()
	set_process(false)

func handle_connecting_signals() -> void:
	register_button.pressed.connect(on_register_pressed)
	login_button.pressed.connect(on_login_pressed)
	back_button.pressed.connect(on_back_button_pressed)
	register_menu.exit_register_menu.connect(on_exit_register_menu)
