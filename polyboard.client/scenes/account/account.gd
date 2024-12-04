class_name Account
extends Control

@onready var account_tab_container = $MarginContainer/VBoxContainer/account_tab_container as Control
@onready var back_button = $MarginContainer/back_button as Button
@onready var username = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/username") as LineEdit
@onready var email = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/email") as LineEdit
@onready var current_pass = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/current_password") as LineEdit
@onready var new_pass = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/new_password") as LineEdit
@onready var confirm_new_pass = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/confirm_new_password") as LineEdit
@onready var save_button = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/save_button") as Button
@onready var error_label = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/error_label") as Label
@onready var http_request = account_tab_container.get_node("TabContainer/Account/MarginContainer/ScrollContainer/HBoxContainer/VBoxContainer/HTTPRequest") as HTTPRequest

var user_id = ""

signal exit_account_menu()

func _ready():
	http_request.connect("request_completed", _on_edit_request_completed, 1)
	save_button.connect("pressed", _on_save_pressed, 1)

	handle_connecting_signals()

func get_user_id_from_jwt() -> String:
	var token = Authentication.token
	print("Token: ", token)
	var jwt_decoder = JwtDecoder.new()
	var decoded_data = jwt_decoder.decode_jwt(token)

	if decoded_data is Dictionary:
		print("USER ID", decoded_data.get("sub", ""))
		return decoded_data.get("sub", "") 
	else:
		print("Error: Failed to decode JWT or userId not found.")
		return ""

func _on_save_pressed() -> void:
	var username_text = str(username.text.strip_edges())
	var email_text = str(email.text.strip_edges())
	var current_pass_text = str(current_pass.text.strip_edges())
	var new_pass_text = str(new_pass.text.strip_edges())
	var confirm_new_pass_text = str(confirm_new_pass.text.strip_edges())
	user_id = get_user_id_from_jwt()
	
	if(new_pass_text != confirm_new_pass_text):
		error_label.text = "Hasla sa niezgodne!"
	
	var user_data = {
		"userId": user_id,
		"userName": username_text,
		"email": email_text,
		"CurrentPassword": current_pass_text,
		"NewPassword": new_pass_text
		
	}
	var json = JSON.new()
	var json_data = json.stringify(user_data)
	#DO ZMIANY!
	var url = SaveManager.url.format({"str": "/edit-profile"})
	var headers = ["Content-Type: application/json"]
	var error = http_request.request(url, headers, HTTPClient.METHOD_PATCH, json_data)

	if error != OK:
		error_label.text = "Failed to send request."
		print("Error sending profile change request: ", error)

func _on_edit_request_completed(result: int, response_code: int, headers: Array, body: PackedByteArray):
	var response_text = body.get_string_from_utf8()
	print("Profile edit response: ", response_text)

	if response_code == 200:
		print("Profile successfully updated.")
		error_label.text = "Profile updated successfully."
		exit_account_menu.emit()
	else:
		var error_message = response_text if response_text != "" else "Failed to update profile."
		error_label.text = "Error: " + error_message

func on_back_button_pressed() -> void:
	exit_account_menu.emit()
	set_process(false)
	
func handle_connecting_signals() -> void:
	back_button.pressed.connect(on_back_button_pressed)
	
