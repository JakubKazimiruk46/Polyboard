extends CanvasLayer

@onready var player_1: MarginContainer = $players/player1
@onready var player_2: MarginContainer = $players/player2
@onready var player_3: MarginContainer = $players/player3
@onready var player_4: MarginContainer = $players/player4
@onready var cost: Label = $VBoxContainer/HBoxContainer/Cost
@onready var next_cost: Label = $VBoxContainer/HBoxContainer2/NextCost
@onready var lidername: Label = $VBoxContainer/HBoxContainer3/lidername

const BASE_COST: int = 100
var cost_amount: int = BASE_COST
var total_time_in_secs : int = 10

func _ready():
	var player_containers = [$players/player1, $players/player2, $players/player3, $players/player4]
	for i in range(player_containers.size()):
		if i < GameData.players.size():
			var player_data = GameData.players[i]
			var nickname_label = player_containers[i].get_node("Panel/VBoxContainer/player%d_nickname" % [i + 1])
			nickname_label.text = player_data["name"]
			player_containers[i].visible = true
		else:
			player_containers[i].visible = false
	if get_parent().has_signal("auction_reset"):
		get_parent().connect("auction_reset", reset_auction_state)
func on_auction_time_timeout():
	total_time_in_secs-=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$VBoxContainer/AuctionTimeLeft.text = '%02d:%02d' % [m, s]
	
	if total_time_in_secs == 0:
		total_time_in_secs = 10
		$AuctionTime.stop()
		self.visible = false
		$VBoxContainer/AuctionTimeLeft.text = ''
		
		var winner_name = lidername.text.strip_edges()
		if winner_name == "":
			print("Brak zwycięzcy aukcji.")
			return
		var game_manager = get_node("/root/Level/GameManager")
		if game_manager == null:
			print("GameManager not found!")
			return
		var winner = game_manager.GetPlayerByName(winner_name)
		var winner_current_position = winner.GetCurrentPositionIndex()
		var currentFigureHead = game_manager.getCurrentPlayer()
		var current_position = currentFigureHead.GetCurrentPositionIndex()
		var Field = game_manager.getCurrentField(current_position)
		if Field and Field.has_method("BuyFieldAfterAuction"):
			Field.BuyFieldAfterAuction(winner, Field, cost_amount)
		else:
			print("Nie można wywołać BuyFieldAfterAuction na Field.")

	
	
func on_auction_button_pressed():
	pass

func _on_auction_button_pressed() -> void:
	pass # Replace with function body.
	
func on_player1_pass_button_pressed():
	player_1.visible = false

func on_player2_pass_button_pressed():
	player_2.visible = false

func on_player3_pass_button_pressed():
	player_3.visible = false

func on_player4_pass_button_pressed():
	player_4.visible = false

func on_player1_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = GameData.players[0]["name"]
	total_time_in_secs = 10

func on_player2_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = GameData.players[1]["name"]
	total_time_in_secs = 10

func on_player3_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = GameData.players[2]["name"]
	total_time_in_secs = 10

func on_player4_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = GameData.players[3]["name"]
	total_time_in_secs = 10

func reset_auction_state():
	cost_amount = BASE_COST
	cost.text = "%d ECTS" %cost_amount
	cost_amount += 50
	next_cost.text = "%d ECTS" % cost_amount
	lidername.text = ""
	var player_containers = [$players/player1, $players/player2, $players/player3, $players/player4]
	for i in range(GameData.players.size()):
		player_containers[i].visible=true
	
