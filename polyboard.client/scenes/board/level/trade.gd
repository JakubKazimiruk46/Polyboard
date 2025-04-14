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
@onready var hover_sound = $HoverSound
@onready var click_sound = $ClickSound

func _ready() -> void:
	# Hide UI on start
	self.visible = false
	
	# Initialize LineEdits with 0
	player1_money_edit.text = "0"
	player2_money_edit.text = "0"
	
	# Connect signals
	player1_money_edit.connect("text_changed", Callable(self, "_on_money1_text_changed"))
	player2_money_edit.connect("text_changed", Callable(self, "_on_money2_text_changed"))
	back_button.connect("mouse_entered", Callable(self, "_on_button_hover"))
	trade_button.connect("mouse_entered", Callable(self, "_on_button_hover"))
	
	# Get reference to game manager
	game_manager = get_node_or_null("/root/Level/GameManager")
	
	# Apply styling to match the main menu
	apply_ui_styling()

func apply_ui_styling():
	# Create fonts and styles that match the main menu
	var impact_font = SystemFont.new()
	impact_font.font_names = PackedStringArray(["Impact"])
	impact_font.subpixel_positioning = 0
	
	var hover_style = StyleBoxFlat.new()
	hover_style.bg_color = Color(0.294118, 1, 0.2, 0.611765)
	hover_style.border_width_left = 2
	hover_style.border_width_top = 2
	hover_style.border_width_right = 2
	hover_style.border_width_bottom = 2
	hover_style.border_color = Color(0.294118, 1, 0.2, 1)
	
	var normal_style = StyleBoxFlat.new()
	normal_style.bg_color = Color(0.6, 0.6, 0.6, 0)
	normal_style.border_width_left = 2
	normal_style.border_width_top = 2
	normal_style.border_width_right = 2
	normal_style.border_width_bottom = 2
	normal_style.border_color = Color(0.294118, 1, 0.2, 1)
	
	# Apply to back button
	back_button.add_theme_font_override("font", impact_font)
	back_button.add_theme_font_size_override("font_size", 20)
	back_button.add_theme_stylebox_override("hover", hover_style)
	back_button.add_theme_stylebox_override("normal", normal_style)
	back_button.modulate = Color(0.293333, 1, 0.2, 1)
	
	# Apply to trade button
	trade_button.add_theme_font_override("font", impact_font)
	trade_button.add_theme_font_size_override("font_size", 20)
	trade_button.add_theme_stylebox_override("hover", hover_style)
	trade_button.add_theme_stylebox_override("normal", normal_style)
	trade_button.modulate = Color(0.293333, 1, 0.2, 1)
	
	# Apply to panel
	var panel = $MarginContainer2/Panel
	var panel_style = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.1, 0.1, 0.1, 0.95)
	panel_style.border_width_left = 3
	panel_style.border_width_top = 3
	panel_style.border_width_right = 3
	panel_style.border_width_bottom = 3
	panel_style.border_color = Color(0.294118, 1, 0.2, 1)
	panel_style.corner_radius_top_left = 8
	panel_style.corner_radius_top_right = 8
	panel_style.corner_radius_bottom_left = 8
	panel_style.corner_radius_bottom_right = 8
	panel.add_theme_stylebox_override("panel", panel_style)
	
	# Apply to option buttons and labels
	player1_fields_option.add_theme_font_override("font", impact_font)
	player2_fields_option.add_theme_font_override("font", impact_font)
	player1_label.add_theme_font_override("font", impact_font)
	
	# Style the input fields
	var line_edit_style = StyleBoxFlat.new()
	line_edit_style.bg_color = Color(0.2, 0.2, 0.2, 0.8)
	line_edit_style.border_width_left = 1
	line_edit_style.border_width_top = 1
	line_edit_style.border_width_right = 1
	line_edit_style.border_width_bottom = 1
	line_edit_style.border_color = Color(0.294118, 1, 0.2, 0.8)
	player1_money_edit.add_theme_stylebox_override("normal", line_edit_style)
	player2_money_edit.add_theme_stylebox_override("normal", line_edit_style)
	
	# Style the labels
	var field_label1 = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/Label
	var money_label1 = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer2/Label
	var field_label2 = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/Label
	var money_label2 = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer2/Label
	
	field_label1.add_theme_font_override("font", impact_font)
	field_label1.modulate = Color(0.293333, 1, 0.2, 1)
	money_label1.add_theme_font_override("font", impact_font)
	money_label1.modulate = Color(0.293333, 1, 0.2, 1)
	field_label2.add_theme_font_override("font", impact_font)
	field_label2.modulate = Color(0.293333, 1, 0.2, 1)
	money_label2.add_theme_font_override("font", impact_font)
	money_label2.modulate = Color(0.293333, 1, 0.2, 1)

func _on_button_hover():
	if hover_sound:
		hover_sound.play()

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
	
	# Update UI with player name
	if is_instance_valid(player1_label) and player1_label:
		player1_label.text = player1_name
	
	# Reset player 2 info
	player2_name = ""
	player2_id = -1
	
	# Get the container
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
	
	# Style the dropdown
	var impact_font = SystemFont.new()
	impact_font.font_names = PackedStringArray(["Impact"])
	option_button.add_theme_font_override("font", impact_font)
	
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
	
	# Reset money fields
	if is_instance_valid(player1_money_edit) and player1_money_edit:
		player1_money_edit.text = "0"
	if is_instance_valid(player2_money_edit) and player2_money_edit:
		player2_money_edit.text = "0"
	
	current_player_ects = 0
	second_player_ects = 0
	
	# Reset field options
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
	if not player1_fields_option or not is_instance_valid(player1_fields_option):
		return
		
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
	if not player2_fields_option or not is_instance_valid(player2_fields_option):
		return
		
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
	if click_sound:
		click_sound.play()
	self.visible = false

func _on_trade_button_pressed():
	if click_sound:
		click_sound.play()
		
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
