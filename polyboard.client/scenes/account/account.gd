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

signal exit_account_menu()

func _ready():
	http_request.connect("request_completed", _on_edit_request_completed, 1)
	save_button.connect("pressed", _on_save_pressed, 1)

func _on_save_pressed() -> void:
	print("Kliknął")
	if username.text.strip_edges() != "":
		print("Wszedł")
		var user_data = {
			"userId": "fc1e5ee3-f124-499f-b6a9-85df2ed88c56", #??????
			"userName": username,
			"email": email,
			"CurrentPassword": current_pass.text.strip_edges(),
			"NewPassword": new_pass.text.strip_edges()
			
		}
		var json = JSON.new()
		var json_data = json.stringify(user_data)
		#DO ZMIANY!
		var url = SaveManager.url.format({"str": "/edit-profile"})
		var headers = ["Content-Type: application/json"]
		var error = http_request.request(url, headers, HTTPClient.METHOD_POST, json_data)
	
		if error != OK:
			error_label.text = "Failed to send request."
			print("Error sending password change request: ", error)
	

func _on_edit_request_completed(result: int, response_code: int, headers: Array, body: PackedByteArray):
	var response_text = body.get_string_from_utf8()
	print("Profile edit response: ", response_text)

	if response_code == 200:
		print("Password successfully updated.")
		current_pass.clear()
		new_pass.clear()
		confirm_new_pass.clear()
		error_label.text = "Password updated successfully."
	else:
		var error_message = response_text if response_text != "" else "Failed to update password."
		error_label.text = "Error: " + error_message

func on_back_button_pressed() -> void:
	exit_account_menu.emit()
	set_process(false)
	
func handle_connecting_signals() -> void:
	back_button.pressed.connect(on_back_button_pressed)
	
