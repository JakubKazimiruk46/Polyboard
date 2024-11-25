extends Control

@onready var scroll_container = $MarginContainer/VBoxContainer/HBoxContainer/ScrollContainer
@onready var lobby_list_container = scroll_container.get_node("VBoxContainer")
@onready var http_request = $HTTPRequest as HTTPRequest

signal join_lobby(lobby_id: String)

func _ready():
	add_refresh_button()
	fetch_lobbies()

func fetch_lobbies() -> void:
	var url = SaveManager.url.format({"str":"/Lobby/lobbies"})
	var error = http_request.request(url, [], HTTPClient.METHOD_GET)
	if error != OK:
		print("Error sending GET request: ", error)
		

func _on_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	if response_code == 200:
		var json_parser = JSON.new()
		var response = json_parser.parse(body.get_string_from_utf8())
		if response.error == OK:
			populate_lobby_list(response.result)
		else:
			print("Błąd parsowania JSON:", response.error_string)
	else:
		print("HTTP Błąd:", response_code)
func add_refresh_button() -> void:
	var refresh_button = Button.new()
	refresh_button.text = "Odśwież"
	refresh_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	$MarginContainer.add_child(refresh_button)
	refresh_button.pressed.connect(func() -> void:
		_on_refresh_button_pressed()
	)
func _on_refresh_button_pressed() -> void:
	print("Odświeżam scenę...")
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
		players_label.text = str(lobby_data["connectedUsersCount"]) + " 👤"
		join_button.text = "Join"

		
		vseperator.modulate = Color(1, 1, 1, 0)
		vseperator.custom_minimum_size = Vector2(30,0)
		name_label.autowrap_mode=false
		name_label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		name_label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		name_label.custom_minimum_size = Vector2(200,0)
		players_label.autowrap_mode=false
		players_label.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		players_label.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		join_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		join_button.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		lobby_list.add_child(lobby_item)
		hseperator.custom_minimum_size = Vector2(0,15)
		hseperator.modulate = Color(1,1,1,0)
		lobby_list.add_child(hseperator)
		join_button.pressed.connect(func() -> void:
			_on_join_button_pressed(lobby_data["id"])
		)
		var privacy_texture = preload("res://assets/images/lock.png")
		privacy_icon.texture = privacy_texture
		if !lobby_data["isPrivate"]:
			privacy_icon.modulate = Color(1, 1, 1, 0)
		lobby_list_container.add_child(lobby_list)

func _on_join_button_pressed(lobby_id: String) -> void:
	emit_signal("join_lobby", lobby_id)
	print("Joining lobby: ", lobby_id)
