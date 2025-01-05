extends Control
class_name Achievements
@onready var tab_container = $".."
@onready var http_request = $MarginContainer/HTTPRequest
@onready var vbox_container = $MarginContainer/VBoxContainer/ScrollContainer/VBoxContainer
@onready var filter = $MarginContainer/VBoxContainer/AchievementFilter
var user_id = ""
var current_filter : String = "All"
func _ready() -> void:
	tab_container.current_tab=0
	http_request.connect("request_completed", _on_request_completed,1)
	filter.connect("filter_changed", _on_filter_changed)
func _process(delta):
	if not self.visible:
		clear_achievements()
func set_filter(filter: String) -> void:
	current_filter = filter
func _on_filter_changed(new_filter: String) -> void:
	print("Filter changed to: ", new_filter)
	set_filter(new_filter)
	clear_achievements()
	load_achievements()
func _visibility_changed() -> void:
	if self.visible:
		print("Achievements tab is now visible")
		load_achievements()
func load_achievements() -> void:
	print("Loading achievements...")
	fetch_achievements()
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
		
func fetch_achievements() -> void:
	var user_id = get_user_id_from_jwt()  
	var url = "http://localhost:8081/Achievement/GetByUserId/%s" % user_id
	var error = http_request.request(url, [], HTTPClient.METHOD_GET)
	if error != OK:
		print("Error sending GET request: ", error)
func clear_achievements():
	for child in vbox_container.get_children():
		vbox_container.remove_child(child)
		child.queue_free() 
func _on_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	var response_text = body.get_string_from_utf8()
	var json = JSON.new()
	var parse_result = json.parse(response_text)
	if parse_result != OK:
		print("JSON parse error:", json.get_error_message())
		return
	
	var achievements = json.data
	if typeof(achievements) != TYPE_ARRAY:
		print("Unexpected data format. Expected Array, got:", typeof(achievements))
		return
	
	if response_code == 200:
		print("Successfully fetched achievements")
		display_user_achievements(achievements)
	else:
		print("HTTP Error:", response_code, "Response:", response_text)

func display_user_achievements(achievements: Array) -> void:
	for achievement in achievements:
		if current_filter == "Achieved" and achievement.get("progress", 0.0) < 100.0:
			continue
		elif current_filter == "Not achieved" and achievement.get("progress", 0.0) == 100.0:
			continue
		var hbox = HBoxContainer.new()
		var texture_rect = TextureRect.new()
		var name_label = Label.new()
		var progress_label = Label.new()
		var description_label = Label.new()

		if achievement.achievement.get("name") == "Finansowy mistrz":
			texture_rect.texture = load("res://assets/images/taxes1.png")
		if achievement.achievement.get("name") == "Budowniczy imperium":
			texture_rect.texture = load("res://assets/images/house.png")
		if achievement.achievement.get("name") == "Strategiczny profesor":
			texture_rect.texture = load("res://assets/images/dollar.png")
		if achievement.achievement.get("name") == "Magnat inwestycyjny":
			texture_rect.texture = load("res://assets/images/apartment.png")
		if achievement.achievement.get("name") == "Bezlitosny":
			texture_rect.texture = load("res://assets/images/bankruptcy.png")
		if achievement.achievement.get("name") == "Milioner":
			texture_rect.texture = load("res://assets/images/coin.png")
		if achievement.achievement.get("name") == "Pierwszy krok":
			texture_rect.texture = load("res://assets/images/success.png")
		if achievement.achievement.get("name") == "Dziekan wydziału inwestycji":
			texture_rect.texture = load("res://assets/images/architect.png")
		if achievement.achievement.get("name") == "Dubletowy szczęściarz":
			texture_rect.texture = load("res://assets/images/dice-game.png")
		if achievement.achievement.get("name") == "Akademicki inwestor":
			texture_rect.texture = load("res://assets/images/investor.png")
		if achievement.achievement.get("name") == "Ekspert ds. dywersyfikacji":
			texture_rect.texture = load("res://assets/images/property.png")
		if achievement.achievement.get("name") == "Podatnik Roku":
			texture_rect.texture = load("res://assets/images/taxes.png")
		if achievement.achievement.get("name") == "Prześladowany przez los":
			texture_rect.texture = load("res://assets/images/jail.png")
			
		name_label.text = achievement.achievement.get("name", "Unknown Achievement")
		progress_label.text = str(achievement.get("progress", 0.0))+"%"
		description_label.text = achievement.achievement.get("requirement", "No description available.")
		name_label.custom_minimum_size = Vector2(220, 0) 
		progress_label.custom_minimum_size = Vector2(40, 0) 
		description_label.custom_minimum_size = Vector2(400, 0) 
		var spacer_1 = Control.new()
		spacer_1.custom_minimum_size = Vector2(20, 0) 
		var spacer_2 = Control.new()
		spacer_2.custom_minimum_size = Vector2(20, 0)
		var spacer_3 = Control.new()
		spacer_3.custom_minimum_size = Vector2(20, 0) 
		
		hbox.add_child(texture_rect)
		hbox.add_child(spacer_1)
		hbox.add_child(name_label)
		hbox.add_child(spacer_2)
		hbox.add_child(progress_label)
		hbox.add_child(spacer_3)
		hbox.add_child(description_label)

		vbox_container.add_child(hbox)
