extends Button

@onready var surrender_button = $"."
var game_manager = null

func _ready() -> void:
	game_manager = $"../../GameManager"
	surrender_button.pressed.connect(on_surrender_button_pressed)

func on_surrender_button_pressed():
	var playerIndex = game_manager.GetCurrentPlayerIndex()
	game_manager.DeclarePlayerBankrupt(playerIndex)
	print("player with index: ", playerIndex, "surrenders")
	
	if game_manager.ShouldGameEnd():
		show_end_game_screen()

func show_end_game_screen():
	var player_results = game_manager.GetPlayerResults()
	var result_message = "Game Over - Player Surrendered"
	
	game_manager.ShowEndGameScreen(result_message)
