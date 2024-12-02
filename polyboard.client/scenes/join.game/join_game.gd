extends Control
class_name JoinGame
@onready var back_button = $MarginContainer/VBoxContainer/back_button as TextureButton
@onready var scroll_container = $MarginContainer/VBoxContainer/HBoxContainer/ScrollContainer
@onready var lobby_list_container = scroll_container.get_node("VBoxContainer")
@onready var http_request = $HTTPRequest as HTTPRequest
@onready var refresh_button = $MarginContainer/VBoxContainer/refresh_button as Button
@onready var hub_service = preload("res://services/HubConnectionService.cs").new() as Node
signal join_lobby(lobby_id: String)
signal exit_joingame_menu

func _ready():
	handle_connecting_signals()
	http_request.connect("request_completed", _on_request_completed, 1)
	
	set_process(false)
	fetch_lobbies()

func fetch_lobbies() -> void:
	var url = "http://localhost:8081/Lobby/lobbies"
	var error = http_request.request(url, [], HTTPClient.METHOD_GET)
	if error != OK:
		print("Error sending GET request: ", error)

func _on_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	var response_text = body.get_string_from_utf8()
	print("Raw response: ", response_text) 
	
	var json = JSON.new()
	var parse_result = json.parse(response_text)
	if parse_result != OK:
		print("JSON parse error: ", json.get_error_message())
		return
	
	var lobbies = json.data  
	if typeof(lobbies) != TYPE_ARRAY:
		print("Unexpected data format. Expected Array, got: ", typeof(lobbies))
		return

	if response_code == 200:
		print("Successfully fetched lobbies: ", lobbies)
		populate_lobby_list(lobbies)
	else:
		print("HTTP Error: ", response_code, " Response: ", response_text)

func _on_refresh_button_pressed() -> void:
	print("Refreshing scene...")
	fetch_lobbies()

func populate_lobby_list(lobbies: Array) -> void:
	for child in lobby_list_container.get_children():
		child.queue_free() 
	var lobby_list = VBoxContainer.new()
	lobby_list.vertical = Control.SIZE_SHRINK_BEGIN
	for lobby_data in lobbies:
		if lobby_data["status"] != 0:
			continue 
		var lobby_item = HBoxContainer.new()
		var privacy_icon = TextureRect.new()
		var name_label = Label.new()
		var players_label = Label.new()
		var join_button = Button.new()
		var vseperator = VSeparator.new()
		var hseperator = HSeparator.new()

		lobby_item.add_child(privacy_icon)
		lobby_item.add_child(name_label)
		lobby_item.add_child(vseperator)
		lobby_item.add_child(players_label)
		lobby_item.add_child(join_button)
		
		name_label.text = lobby_data["lobbyName"]
		players_label.text = str(lobby_data.get("connectedUserCount")) + " 👤"
		join_button.text = "Join"
		
		vseperator.modulate = Color(1, 1, 1, 0)
		vseperator.custom_minimum_size = Vector2(30,0)
		name_label.autowrap_mode = false
		name_label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		name_label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		name_label.custom_minimum_size = Vector2(200,0)
		players_label.autowrap_mode = false
		players_label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		players_label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		join_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		join_button.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		lobby_list.add_child(lobby_item)
		hseperator.custom_minimum_size = Vector2(0,15)
		hseperator.modulate = Color(1,1,1,0)
		lobby_list.add_child(hseperator)
		
		join_button.pressed.connect(func() -> void:
			_on_join_button_pressed(lobby_data["id"], lobby_data["isPrivate"])
		)

		var privacy_texture = preload("res://assets/images/lock.png")
		privacy_icon.texture = privacy_texture
		if !lobby_data["isPrivate"]:
			privacy_icon.modulate = Color(1, 1, 1, 0)
		lobby_list_container.add_child(lobby_list)
		
func change_to_lobby_scene(lobby_id):
	print("Joining lobby with ID: ", lobby_id)
	# Załaduj scenę Lobby
	var lobby_scene = preload("res://scenes/lobby/Lobby.tscn")
	var lobby_instance = lobby_scene.instantiate()
	# Ustaw LobbyId w nowej scenie, jeśli metoda istnieje
	if lobby_instance.has_method("set_lobby_id"):
		lobby_instance.set_lobby_id(lobby_id)
	# Usuń bieżącą scenę i dodaj nową
	var current_scene = get_tree().current_scene
	# Usuń bieżącą scenę (odroczenie usuwania)
	if current_scene != null:
		current_scene.call_deferred("free")
	# Ustaw nową scenę
	get_tree().root.add_child(lobby_instance)
	get_tree().current_scene = lobby_instance
	
		
		

func _on_join_button_pressed(lobby_id: String, isPrivate: bool) -> void:
	if !isPrivate:
		print("Joining public lobby: ", lobby_id)
		hub_service.call("JoinLobby", lobby_id, null)  
		emit_signal("join_lobby", lobby_id)
		change_to_lobby_scene(lobby_id)
	else:
		print("Joining private lobby. Requesting password...")
		var password_popup = Popup.new()
		password_popup.name = "PasswordPopup"
		password_popup.title = "Enter Password"
		password_popup.add_theme_color_override("bg_color", Color(0, 0, 0, 0.8))  
		add_child(password_popup)

		var panel = Panel.new()
		panel.add_theme_color_override("bg_color", Color(0.2, 0.2, 0.2))  
		panel.custom_minimum_size = Vector2(260, 120) 
		password_popup.add_child(panel)

		var vbox = VBoxContainer.new()
		vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		vbox.size_flags_vertical = Control.SIZE_EXPAND_FILL
		panel.add_child(vbox)
		vbox.custom_minimum_size = Vector2(260, 140)

		var label = Label.new()
		label.text = "Enter the Lobby Password"
		label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		vbox.add_child(label)

		var password_line_edit = LineEdit.new()
		password_line_edit.placeholder_text = "Password"
		password_line_edit.secret = true
		password_line_edit.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		password_line_edit.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		password_line_edit.add_theme_color_override("focus_color", Color(0.5, 0.5, 0.5))  
		password_line_edit.add_theme_color_override("font_color", Color(1, 1, 1)) 
		password_line_edit.custom_minimum_size = Vector2(250, 40)  
		vbox.add_child(password_line_edit)

		var ok_button = Button.new()
		ok_button.text = "OK"
		ok_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		ok_button.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		ok_button.add_theme_color_override("font_color", Color(0, 1, 0))  
		ok_button.custom_minimum_size = Vector2(150, 40)
		ok_button.connect("pressed", func() -> void:
			var entered_password = password_line_edit.text.strip_edges()
			if entered_password == "":
				print("Password cannot be empty!")
			else:
				print("Joining private lobby: ", lobby_id, " with password: ", entered_password)
				hub_service.call("JoinLobby", lobby_id, null)
				emit_signal("join_lobby", lobby_id, entered_password)
				change_to_lobby_scene(lobby_id)
				password_popup.queue_free()
		)
		vbox.add_child(ok_button)
		password_popup.popup_centered()


func _on_back_button_pressed() -> void:
	exit_joingame_menu.emit()
	print("Going back to main menu...")
	set_process(false)
	
func handle_connecting_signals() -> void:
	back_button.pressed.connect(_on_back_button_pressed)
	refresh_button.pressed.connect(_on_refresh_button_pressed)
	
