extends CanvasLayer

var player1_name: String = ""  
var player1_id: int = -1      
var player2_name: String = ""  
var player2_id: int = -1      
var selected_field_id: int = -1  
var all_players = []
var game_manager = null
var current_player_ects: int = 0
var second_player_ects: int = 0

# UI elements
@onready var player_selector = $MarginContainer2/Panel/MarginContainer/VBoxContainer/Label
@onready var player1_label = $MarginContainer2/Panel/MarginContainer/VBoxContainer/Label2
@onready var player1_fields_option = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/OptionButton
@onready var player2_fields_option = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/OptionButton
@onready var player1_money_edit = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer2/LineEdit
@onready var player2_money_edit = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer2/LineEdit
@onready var trade_button = $Button2
@onready var back_button = $Button

func _ready() -> void:
	# Hide UI on start
	self.visible = false
	
	# Initialize LineEdits with 0
	player1_money_edit.text = "0"
	player2_money_edit.text = "0"
	
	# Connect text change signals
	player1_money_edit.connect("text_changed", Callable(self, "_on_money1_text_changed"))
	player2_money_edit.connect("text_changed", Callable(self, "_on_money2_text_changed"))
	
	# Get reference to game manager
	game_manager = get_node_or_null("/root/Level/GameManager")

func _on_money1_text_changed(new_text: String) -> void:
	# Ensure input is a valid number
	if new_text.is_empty():
		player1_money_edit.text = "0"
		current_player_ects = 0
		return
		
	if not new_text.is_valid_int():
		player1_money_edit.text = "0"
		current_player_ects = 0
		return
		
	var value = int(new_text)
	
	# Ensure value is not negative
	if value < 0:
		player1_money_edit.text = "0"
		current_player_ects = 0
		return
		
	# Check if player has enough ECTS - using a safer method
	if game_manager and player1_id >= 0:
		# Try to get player ECTS directly from the game manager
		var player_ects = game_manager.call("GetPlayerECTS", player1_id)
		
		if player_ects != null and value > player_ects:
			# Set to maximum available
			player1_money_edit.text = str(player_ects)
			current_player_ects = player_ects
			show_notification("Maksymalna dostępna ilość ECTS: " + str(player_ects), 2.0)
			return
	
	current_player_ects = value

func _on_money2_text_changed(new_text: String) -> void:
	# Ensure input is a valid number
	if new_text.is_empty():
		player2_money_edit.text = "0"
		second_player_ects = 0
		return
		
	if not new_text.is_valid_int():
		player2_money_edit.text = "0"
		second_player_ects = 0
		return
		
	var value = int(new_text)
	
	# Ensure value is not negative
	if value < 0:
		player2_money_edit.text = "0"
		second_player_ects = 0
		return
	
	# Check if player has enough ECTS - using a safer method
	if game_manager and player2_id >= 0:
		# Try to get player ECTS directly from the game manager
		var player_ects = game_manager.call("GetPlayerECTS", player2_id)
		
		if player_ects != null and value > player_ects:
			# Set to maximum available
			player2_money_edit.text = str(player_ects)
			second_player_ects = player_ects
			show_notification("Maksymalna dostępna ilość ECTS: " + str(player_ects), 2.0)
			return
	
	second_player_ects = value

func setup_trade(current_player_name, player_data, field_id):
	# Store player data
	player1_name = current_player_name
	selected_field_id = field_id
	all_players = player_data
	
	# Find current player ID
	for i in range(player_data.size()):
		if player_data[i].name == current_player_name:
			player1_id = i
			break
	
	# Update UI with player name - check if the label exists first
	if is_instance_valid(player1_label) and player1_label:
		player1_label.text = player1_name
	
	# Reset player 2 info
	player2_name = ""
	player2_id = -1
	
	# Get the container without assuming it exists
	var container = $MarginContainer2/Panel/MarginContainer/VBoxContainer
	if not container:
		print_debug("Container not found!")
		return
	
	# Check if PlayerSelector already exists
	var existing_selector = container.get_node_or_null("PlayerSelector")
	if existing_selector:
		existing_selector.queue_free()
	
	# Create dropdown for player selection
	var option_button = OptionButton.new()
	option_button.name = "PlayerSelector"
	option_button.custom_minimum_size = Vector2(150, 0)
	option_button.size_flags_horizontal = 3  # EXPAND_FILL
	option_button.connect("item_selected", Callable(self, "_on_player_selected"))
	
	# Add it to the container at index 0
	container.add_child(option_button)
	if container.get_child_count() > 1:
		container.move_child(option_button, 0)
	
	# Populate the dropdown
	option_button.clear()
	option_button.add_item("Wybierz gracza", -1)  # Default option
	
	for player in all_players:
		if player.name != player1_name:
			option_button.add_item(player.name, player.id)
	
	# Reset money fields - check if they exist first
	if is_instance_valid(player1_money_edit) and player1_money_edit:
		player1_money_edit.text = "0"
	if is_instance_valid(player2_money_edit) and player2_money_edit:
		player2_money_edit.text = "0"
	
	current_player_ects = 0
	second_player_ects = 0
	
	# Reset field options - check if they exist first
	if is_instance_valid(player1_fields_option) and player1_fields_option:
		player1_fields_option.clear()
	if is_instance_valid(player2_fields_option) and player2_fields_option:
		player2_fields_option.clear()
	
	# Populate current player's fields
	populate_player1_fields()
	
	# Show the dialog
	self.visible = true

func _on_player_selected(index):
	# Get the option button in a more reliable way
	var option_button = get_node_or_null("MarginContainer2/Panel/MarginContainer/VBoxContainer/PlayerSelector")
	
	# If the option button doesn't exist, try to find it as a child of the container
	if not option_button:
		var container = get_node_or_null("MarginContainer2/Panel/MarginContainer/VBoxContainer")
		if container:
			option_button = container.get_node_or_null("PlayerSelector")
	
	# If we still can't find it, abort
	if not option_button:
		print_debug("PlayerSelector not found!")
		return
	
	# If first option (Wybierz gracza) was selected
	if index == 0:
		player2_id = -1
		player2_name = ""
		player2_fields_option.clear()
		return
	
	# Get the selected player's ID and name
	player2_id = option_button.get_item_id(index)
	player2_name = option_button.get_item_text(index)
	
	# Update the label text
	if player_selector and is_instance_valid(player_selector):
		player_selector.text = player2_name
	
	# Populate player2's fields
	populate_player2_fields()
	
	# Reset player2's money
	if player2_money_edit and is_instance_valid(player2_money_edit):
		player2_money_edit.text = "0"
	second_player_ects = 0

func populate_player1_fields():
	# Clear existing options
	player1_fields_option.clear()
	
	# Add "None" option
	player1_fields_option.add_item("Brak", -1)
	
	# Get board reference
	var board = get_node_or_null("/root/Level/Board")
	if not board:
		print_debug("Board not found")
		return
	
	# Call a method on the board to get fields owned by a player name
	var owned_fields = board.call("GetFieldsOwnedByPlayerName", player1_name)
	
	# Owned_fields will be an array of dictionaries with fieldId and fieldName
	if owned_fields:
		for field_info in owned_fields:
			player1_fields_option.add_item(field_info["name"], field_info["id"])
	
	# If a specific field was selected for trade, select it
	if selected_field_id >= 0:
		for i in range(player1_fields_option.get_item_count()):
			if player1_fields_option.get_item_id(i) == selected_field_id:
				player1_fields_option.select(i)
				break

func populate_player2_fields():
	# Clear existing options
	player2_fields_option.clear()
	
	# Add "None" option
	player2_fields_option.add_item("Brak", -1)
	
	# Get board reference
	var board = get_node_or_null("/root/Level/Board")
	if not board:
		print_debug("Board not found")
		return
	
	# Call a method on the board to get fields owned by a player name
	var owned_fields = board.call("GetFieldsOwnedByPlayerName", player2_name)
	
	# Owned_fields will be an array of dictionaries with fieldId and fieldName
	if owned_fields:
		for field_info in owned_fields:
			player2_fields_option.add_item(field_info["name"], field_info["id"])

func _on_back_button_pressed():
	self.visible = false

func _on_trade_button_pressed():
	if player2_id < 0:
		show_error("Wybierz gracza do wymiany!", 3.0)
		return
	
	# Get selected fields
	var p1_field_id = -1
	var p2_field_id = -1
	
	if player1_fields_option.selected > 0:  # Skip "None" option
		p1_field_id = player1_fields_option.get_item_id(player1_fields_option.selected)
	
	if player2_fields_option.selected > 0:  # Skip "None" option
		p2_field_id = player2_fields_option.get_item_id(player2_fields_option.selected)
	
	# Get ECTS amounts
	var p1_ects = int(player1_money_edit.text) if player1_money_edit.text.is_valid_int() else 0
	var p2_ects = int(player2_money_edit.text) if player2_money_edit.text.is_valid_int() else 0
	
	# Check if there's anything to trade
	if p1_field_id < 0 and p2_field_id < 0 and p1_ects == 0 and p2_ects == 0:
		show_error("Wymiana musi zawierać co najmniej jeden element!", 3.0)
		return
	
	# Process the trade
	if game_manager:
		game_manager.call("process_trade", player1_id, player2_id, p1_field_id, p2_field_id, p1_ects, p2_ects)
	
	# Hide the trade UI
	self.visible = false

func show_notification(message: String, duration: float = 3.0) -> void:
	var notifications = get_node_or_null("/root/Notifications")
	if notifications:
		notifications.call("show_notification", message, duration)

func show_error(message: String, duration: float = 3.0) -> void:
	var notifications = get_node_or_null("/root/Notifications")
	if notifications:
		notifications.call("show_error", message, duration)
