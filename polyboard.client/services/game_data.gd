extends Node

# Lista graczy: [{ "name": "Player1", "skin": 0 }, ...]
var players: Array = []
var current_player_editing_skin_id: int = 0

const MIN_PLAYERS = 2
const MAX_PLAYERS = 4

func _ready():
	reset_data()

func reset_data():
	players.clear()
	for i in range(MIN_PLAYERS):
		players.append({ "name": "Player" + str(i + 1), "skin": 0 })  # Domyślnie gracze mają skin o ID 0

func add_player():
	if players.size() < MAX_PLAYERS:
		players.append({ "name": "Player" + str(players.size() + 1), "skin": 0 })

func remove_player():
	if players.size() > MIN_PLAYERS:
		players.pop_back()

func set_player_name(index: int, new_name: String):
	if index >= 0 and index < players.size():
		players[index]["name"] = new_name

func set_player_skin(skin_id: int):
	if current_player_editing_skin_id >= 0 and current_player_editing_skin_id < players.size():
		players[current_player_editing_skin_id]["skin"] = skin_id
		
func get_player_count() -> int:	
	return players.size()
	
func get_last_player_id() -> int:	
	return players.size() - 1
	
func print_player_list():
	for player in players:
		print("Player: " + player["name"] + ", Skin ID: " + str(player["skin"]))

func get_player_name_by_id(player_id: int) -> String:
	if player_id >= 0 and player_id < players.size():
		return players[player_id]["name"]
	else:
		return ""

func get_player_skin_id_by_id(player_id: int) -> int:
	if player_id >= 0 and player_id < players.size():
		return players[player_id]["skin"]
	else:
		return -1

func set_current_player_editing_skin(player_id: int):
	if player_id >= 0 and player_id < players.size():
		current_player_editing_skin_id = player_id
		print("Gracz edytujący skin to: " + players[player_id]["name"])
	else:
		print("Niepoprawne ID gracza!")
		
func get_current_player_editing_skin() -> int:
	return current_player_editing_skin_id
