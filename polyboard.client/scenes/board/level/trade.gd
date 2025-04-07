extends CanvasLayer

var player1_name: String = ""  
var player1_id: int = -1      
var player2_name: String = ""  
var player2_id: int = -1      
var selected_field_id: int = -1  
var all_players = []  

func _ready() -> void:

	self.visible = false

func setup_trade(current_player_name, player_data, field_id):
	player1_name = current_player_name
	selected_field_id = field_id
	all_players = player_data

	$MarginContainer2/Panel/MarginContainer/VBoxContainer/Label2.text = player1_name

	var player_selector = $MarginContainer2/Panel/MarginContainer/VBoxContainer/Label
	player_selector.text = "Select Player"

	var option_button = $MarginContainer2/Panel/MarginContainer/VBoxContainer/PlayerSelector
	if not option_button:
		option_button = OptionButton.new()
		option_button.name = "PlayerSelector"
		$MarginContainer2/Panel/MarginContainer/VBoxContainer.add_child(option_button)
		option_button.connect("item_selected", Callable(self, "_on_player_selected"))

	option_button.clear()

	for player in all_players:
		if player.name != player1_name:
			option_button.add_item(player.name, player.id)

	self.visible = true

func _on_player_selected(index):

	var option_button = $MarginContainer2/Panel/MarginContainer/VBoxContainer/PlayerSelector
	player2_id = option_button.get_item_id(index)
	player2_name = option_button.get_item_text(index)

	$MarginContainer2/Panel/MarginContainer/VBoxContainer/Label.text = player2_name
	
	# Populate field options
	populate_field_options()

func populate_field_options():
	var p1_fields = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/OptionButton
	var p2_fields = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/OptionButton
	
	p1_fields.clear()
	p2_fields.clear()
	
	var board = get_node("/root/Level/Board")
	
	var game_manager = get_node("/root/Level/GameManager")
	var current_player = game_manager.call("getCurrentPlayer")

	for i in range(40): 
		var field = board.call("GetFieldById", i)
		if field and field.owned and field.Owner and field.Owner.Name == player1_name:
			p1_fields.add_item(field.Name, i)

	if player2_id >= 0:
		var all_players = game_manager.get("Players")
		if all_players and player2_id < all_players.size():
			var player2 = all_players[player2_id]
			
			for i in range(40):
				var field = board.call("GetFieldById", i)
				if field and field.owned and field.Owner and field.Owner.Name == player2_name:
					p2_fields.add_item(field.Name, i)
	
	if selected_field_id >= 0:
		for i in range(p1_fields.get_item_count()):
			if p1_fields.get_item_id(i) == selected_field_id:
				p1_fields.select(i)
				break

func _on_back_button_pressed():
	self.visible = false
	
func _on_trade_button_pressed():
	if player2_id < 0:
		var notifications = get_node("/root/Notifications")
		if notifications:
			notifications.call("show_error", "Please select a player to trade with!", 3.0)
		return

	var p1_field_option = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/OptionButton
	var p2_field_option = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/OptionButton
	
	var p1_money = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer2/LineEdit.text
	var p2_money = $MarginContainer2/Panel/MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer2/LineEdit.text

	var p1_field_id = -1
	var p2_field_id = -1
	
	if p1_field_option.selected >= 0:
		p1_field_id = p1_field_option.get_item_id(p1_field_option.selected)
	
	if p2_field_option.selected >= 0:
		p2_field_id = p2_field_option.get_item_id(p2_field_option.selected)
	
	var game_manager = get_node("/root/Level/GameManager")
	if game_manager:
		var p1_ects = int(p1_money) if p1_money.is_valid_int() else 0
		var p2_ects = int(p2_money) if p2_money.is_valid_int() else 0

		game_manager.call("process_trade", player1_id, player2_id, p1_field_id, p2_field_id, p1_ects, p2_ects)
	
	# Hide the trade UI
	self.visible = false
