extends CanvasLayer

@onready var player_1: MarginContainer = $players/player1
@onready var player_2: MarginContainer = $players/player2
@onready var player_3: MarginContainer = $players/player3
@onready var player_4: MarginContainer = $players/player4
@onready var cost: Label = $VBoxContainer/HBoxContainer/Cost
@onready var next_cost: Label = $VBoxContainer/HBoxContainer2/NextCost
@onready var lidername: Label = $VBoxContainer/HBoxContainer3/lidername


var cost_amount: int = 100
var total_time_in_secs : int = 10

func _ready():
	cost.text = '%d ECTS' % [cost_amount]
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	
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
	lidername.text = 'JACEK'
	total_time_in_secs = 10

func on_player2_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = 'KUBA'
	total_time_in_secs = 10

func on_player3_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = 'ŁUKASZ'
	total_time_in_secs = 10

func on_player4_bid_button_pressed():
	cost.text = $VBoxContainer/HBoxContainer2/NextCost.text
	cost_amount += 50
	next_cost.text = '%d ECTS' % [cost_amount]
	lidername.text = 'DAWID'
	total_time_in_secs = 10
