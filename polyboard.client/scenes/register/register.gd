class_name Register
extends Control


#zmienne z przycisków
@onready var back_button = $MarginContainer/HBoxContainer/VBoxContainer/back_button as Button
@onready var username_input = $MarginContainer/HBoxContainer/VBoxContainer/username_input as TextEdit
@onready var email_input = $MarginContainer/HBoxContainer/VBoxContainer/email_input as TextEdit
@onready var password_input = $MarginContainer/HBoxContainer/VBoxContainer/password_input as LineEdit
@onready var confirm_password_input = $MarginContainer/HBoxContainer/VBoxContainer/confirm_password_input as LineEdit
@onready var register_button = $MarginContainer/HBoxContainer/VBoxContainer/register_button as Button
@onready var error_label = $MarginContainer/HBoxContainer/VBoxContainer/error_label as Label
#Kontaktowanie sie z serwerem
@onready var http_request = $HTTPRequest as HTTPRequest



signal exit_register_menu

func _ready():
	back_button.button_down.connect(on_back_button_pressed)
	register_button.button_down.connect(on_register_button_pressed)
	set_process(false)
	http_request.connect("request_completed", _on_request_completed, 1)
	set_process(false)
	
func on_back_button_pressed() -> void:
	exit_register_menu.emit()
	set_process(false)




func on_register_button_pressed() -> void:
	#Zmienne z pól. strip_edges usuwa białe znaki z końca
	var username = username_input.text.strip_edges()
	var email = email_input.text.strip_edges()
	var password = password_input.text.strip_edges()
	var confirm_password = confirm_password_input.text.strip_edges()

	error_label.text = ""
	#Errory
	if username == "":
		error_label.text = "Username cannot be empty."
		return

	if email == "":
		error_label.text = "Email cannot be empty."
		return

	if password == "":
		error_label.text = "Password cannot be empty."
		return

	if password != confirm_password:
		error_label.text = "Passwords do not match."
		return
		
	var registatrion_data = {
		"UserName": username,
		"Email": email,
		"Password": password
		}
	var json = JSON.new()
	var json_data = json.stringify(registatrion_data)
	var url = SaveManager.url.format({"str":"/register"})
	
	var headers = ["Content-Type: application/json"]
	var error = http_request.request(url, headers, HTTPClient.METHOD_POST, json_data)
	
	if error != OK:
		error_label.text = "Failed to send request."
		print("Error sending request: ", error)
func _on_request_completed(result: int, response_code: int, headers: Array, body: PackedByteArray):
	var response_text = body.get_string_from_utf8()
	var json = JSON.new()
	print("Raw response text: ", response_text)
	var response = json.parse(response_text)
	
	if response != OK:
		error_label.text = "Failed to parse response."
		print("Error parsing response: ", response)
		return

	if response == OK:
		print("Registration successful: ", response)
		error_label.text = "Registration successful!"
		exit_register_menu.emit()
		set_process(false)
	else:
		error_label.text = "Registration failed: " + response.result["message"]
