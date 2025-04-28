extends Node
class_name AchievementManager

const ACHIEVEMENTS_PATH = "res://data/achievements.json"
const PROGRESS_PATH = "res://data/progress.json"

signal achievement_unlocked(achievement_name)
signal achievement_progress(achievement_name, progress_percent)

var achievements: Dictionary = {} 
var progress_data: Dictionary = {} 
var game_state: Dictionary = {}

func _ready():
	load_achievements()
	load_progress()
	reset_game_state()

func load_achievements():
	var file = FileAccess.open(ACHIEVEMENTS_PATH, FileAccess.READ)
	if file:
		var content = file.get_as_text()
		var parsed = JSON.parse_string(content)
		if parsed:
			for achievement in parsed:
				achievements[achievement["name"]] = achievement
		else:
			print("Błąd parsowania achievements.json")
	else:
		print("Nie można otworzyć pliku achievements.json")

func load_progress():
	if FileAccess.file_exists(PROGRESS_PATH):
		var file = FileAccess.open(PROGRESS_PATH, FileAccess.READ)
		if file:
			var content = file.get_as_text()
			var parsed = JSON.parse_string(content)
			if parsed:
				progress_data = parsed
			else:
				print("Błąd parsowania progress.json")
		else:
			print("Nie można otworzyć pliku progress.json")
			initialize_progress()
	else:
		initialize_progress()

func initialize_progress():
	for name in achievements.keys():
		progress_data[name] = 0.0
	save_progress()

func save_progress():
	var file = FileAccess.open(PROGRESS_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(progress_data, "\t"))
		file.close()
		print(game_state)
		print("Zapisano postęp!")
	else:
		print("Nie można zapisać pliku progress.json")

func get_achievements() -> Array:
	var result = []

	for name in achievements.keys():
		var ach = achievements[name]
		var raw_progress = progress_data.get(name, 0.0)
		var number_to_reach = ach.get("number_to_reach", null)

		var percent := 100.0
		if number_to_reach != null and number_to_reach > 0.0:
			percent = clamp((raw_progress / number_to_reach) * 100.0, 0.0, 100.0)

		result.append({
			"achievement": ach,
			"progress": percent,
			"raw_progress": raw_progress,
			"is_completed": percent >= 100.0
		})

	return result

func add_progress(name: String, value: float):
	if not achievements.has(name):
		print("Nie znaleziono osiągnięcia:", name)
		return

	var current = progress_data.get(name, 0.0)
	var new_progress = current + value
	progress_data[name] = new_progress
	save_progress()
	
	var achievement = achievements[name]
	var number_to_reach = achievement.get("number_to_reach", 1.0)
	var progress_percent = clamp((new_progress / number_to_reach) * 100.0, 0.0, 100.0)
	
	emit_signal("achievement_progress", name, progress_percent)
	
	if new_progress >= number_to_reach and current < number_to_reach:
		emit_signal("achievement_unlocked", name)

func reset_game_state():
	game_state = {
		"dean_office_visits": 0,
		"current_game_hotels_same_color": {},
		"current_game_properties_by_color": {},
		"current_game_buildings_built": 0,
		"current_game_taxes_paid": 0,
		"current_game_owned_facilities": 0,
		"current_game_properties_value": 0,
		"consecutive_doubles": 0,
		"current_game_owned_colors": [],
		"current_game_penalties_paid": false,
		"current_game_bankrupted_turn": 0,
		"owned_properties_colors": {}
	}
func track_dean_office_visit():
	game_state["dean_office_visits"] += 1
	if game_state["dean_office_visits"] >= 3:
		add_progress("Prześladowany przez los", 1.0)

func track_property_purchase(department: String, value: int, is_facility: bool = false):
	if not game_state["current_game_properties_by_color"].has(department):
		game_state["current_game_properties_by_color"][department] = 0
	
	game_state["current_game_properties_by_color"][department] += 1
	game_state["current_game_properties_value"] += value
	
	if is_facility:
		game_state["current_game_owned_facilities"] += 1
		if game_state["current_game_owned_facilities"] >= 6:

			add_progress("Akademicki inwestor", 1.0)
	
	var colors_owned_complete = 0
	for c in game_state["current_game_properties_by_color"]:
		if game_state["current_game_properties_by_color"][c] >= 3:
			colors_owned_complete += 1
			if not c in game_state["current_game_owned_colors"]:
				game_state["current_game_owned_colors"].append(c)
	
	if colors_owned_complete >= 3:
		add_progress("Magnat inwestycyjny", 1.0)
	
	if game_state["current_game_owned_colors"].size() >= 8:
		add_progress("Ekspert ds. dywersyfikacji", 1.0)
	
	if game_state["current_game_properties_value"] >= 3000:
		add_progress("Strategiczny profesor", 1.0)

func track_hotel_built(department: String):
	if not game_state["current_game_hotels_same_color"].has(department):
		game_state["current_game_hotels_same_color"][department] = 0
	
	game_state["current_game_hotels_same_color"][department] += 1
	game_state["current_game_buildings_built"] += 1
	
	if game_state["current_game_hotels_same_color"][department] >= 3:
		add_progress("Dziekan wydziału inwestycji", 1.0)
	
	if game_state["current_game_buildings_built"] >= 20:
		add_progress("Budowniczy imperium", 1.0)

func track_tax_payment(amount: int):
	game_state["current_game_taxes_paid"] += amount
	print(game_state)
	if game_state["current_game_taxes_paid"] >= 2000:
		add_progress("Podatnik Roku", 1.0)
	
	game_state["current_game_penalties_paid"] = true

func track_dice_roll(is_double: bool):
	if is_double:
		game_state["consecutive_doubles"] += 1
		print(game_state)
		if game_state["consecutive_doubles"] >= 3:
			add_progress("Dubletowy szczęściarz", 1.0)
	else:
		game_state["consecutive_doubles"] = 0

func track_bankruptcy(turn: int):
	if turn <= 5:
		print(game_state)
		add_progress("Bezlitosny", 1.0)

func track_game_win(turn: int):
	print(game_state)
	add_progress("Pierwszy krok", 1.0)

	if not game_state["current_game_penalties_paid"]:
		add_progress("Finansowy mistrz", 1.0)

func track_money_earned(amount: int):
	add_progress("Milioner", amount)
