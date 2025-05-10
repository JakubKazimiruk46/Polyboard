extends Node
class_name AchievementManager

const ACHIEVEMENTS_PATH = "res://data/achievements.json"
const SAVE_DIR_NAME = "PolyBoard"
const PROGRESS_FILENAME = "progress.json"

signal achievement_unlocked(achievement_name)
signal achievement_progress(achievement_name, progress_percent)

var PROGRESS_PATH = get_custom_save_path()
var achievements: Dictionary = {} 
var progress_data: Dictionary = {} 
var game_state: Dictionary = {}
var notificationService = NotificationService

func _ready():
	load_achievements()
	load_progress()
	reset_game_state()
	print(notificationService)
	print("Wczytany postęp:", progress_data)
func get_custom_save_path() -> String:
	var docs_path = OS.get_system_dir(OS.SYSTEM_DIR_DOCUMENTS)
	var full_path = docs_path.path_join(SAVE_DIR_NAME)
	DirAccess.make_dir_recursive_absolute(full_path)
	return full_path.path_join(PROGRESS_FILENAME)

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
	print("Inicjalizacja postępu...")
	for name in achievements.keys():
		progress_data[name] = 0.0
	print("Zainicjalizowano dane:", progress_data)
	save_progress()

func save_progress():
	var file = FileAccess.open(PROGRESS_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(progress_data, "\t"))
		file.close()
		print("Zapisano postęp!")
	else:
		print("Nie można zapisać pliku progress.json")

func get_achievements() -> Array:
	var result = []

	for name in achievements.keys():
		var ach = achievements[name]
		var raw_progress = progress_data.get(name, 0.0)
		var number_to_reach = ach.get("number_to_reach", 1.0)

		if number_to_reach <= 0:
			number_to_reach = 1.0

		var percent = clamp((raw_progress / number_to_reach) * 100.0, 0.0, 100.0)

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
	if number_to_reach <= 0:
		number_to_reach = 1.0

	var progress_percent = clamp((new_progress / number_to_reach) * 100.0, 0.0, 100.0)
	print("Dodano postęp do:", name, "Procent:", progress_percent)

	emit_signal("achievement_progress", name, progress_percent)
	notificationService.ShowProgress(name, progress_percent)

	if new_progress >= number_to_reach and current < number_to_reach:
		emit_signal("achievement_unlocked", name)
		NotificationService.ShowAchievement(name)


func reset_game_state():
	game_state = {
		"dean_office": {
			"visits": 0
		},
		"properties": {
			"value": 0,
			"by_department": {},
			"owned_departments": [],
			"facilities_owned": 0
		},
		"buildings": {
			"built": 0,
			"hotels_by_department": {}  
		},
		"taxes_paid": 0,
		"penalties_paid": false,
		"consecutive_doubles": 0,
		"bankrupted_turn": 0
	}


func add_once(name: String, condition: bool):
	if not condition:
		return

	var current_progress = progress_data.get(name, 0.0)
	if current_progress >= 1.0:
		return

	add_progress(name, 1.0)
	
	var achievement = achievements.get(name, null)
	if achievement:
		var number_to_reach = achievement.get("number_to_reach", 1.0)
		if number_to_reach <= 0.0:
			number_to_reach = 1.0

		var new_progress = progress_data[name]
		var percent = clamp((new_progress / number_to_reach) * 100.0, 0.0, 100.0)

		if percent >= 100.0:
			if Engine.has_singleton("NotificationService"):
				Engine.get_singleton("NotificationService").ShowAchievement(name)
		else:
			if Engine.has_singleton("NotificationService"):
				Engine.get_singleton("NotificationService").ShowProgress(name, percent)

func track_dean_office_visit():
	game_state["dean_office"]["visits"] += 1
	add_once("Prześladowany przez los", game_state["dean_office"]["visits"] >= 3)

func track_property_purchase(department: String, value: int, is_facility: bool = false):
	var props = game_state["properties"]

	if not props.has("by_department"):
		props["by_department"] = {}
	if not props.has("owned_departments"):
		props["owned_departments"] = []

	if not props["by_department"].has(department):
		props["by_department"][department] = 0
	props["by_department"][department] += 1
	props["value"] += value

	if is_facility:
		props["facilities_owned"] += 1
		add_once("Akademicki inwestor", props["facilities_owned"] >= 6)

	var completed_departments = 0
	for d in props["by_department"]:
		if props["by_department"][d] >= 3:
			completed_departments += 1
			if not props["owned_departments"].has(d):
				props["owned_departments"].append(d)

	add_once("Magnat inwestycyjny", completed_departments >= 3)
	add_once("Ekspert ds. dywersyfikacji", props["owned_departments"].size() >= 8)
	add_once("Strategiczny profesor", props["value"] >= 3000)


func track_hotel_built(department: String):
	var build = game_state["buildings"]

	if not build.has("hotels_by_department"):
		build["hotels_by_department"] = {}

	var hotels = build["hotels_by_department"]

	if not hotels.has(department):
		hotels[department] = 0

	hotels[department] += 1
	build["built"] += 1

	add_once("Dziekan wydziału inwestycji", hotels[department] >= 3)
	add_once("Budowniczy imperium", build["built"] >= 20)

func track_tax_payment(amount: int):
	game_state["taxes_paid"] += amount
	game_state["penalties_paid"] = true
	add_once("Podatnik Roku", game_state["taxes_paid"] >= 2000)

func track_dice_roll(is_double: bool):
	if is_double:
		game_state["consecutive_doubles"] += 1
		add_once("Dubletowy szczęściarz", game_state["consecutive_doubles"] >= 3)
	else:
		game_state["consecutive_doubles"] = 0

func track_bankruptcy(turn: int):
	add_once("Bezlitosny", turn <= 5)

func track_game_win(turn: int):
	add_once("Pierwszy krok", true)
	add_once("Finansowy mistrz", not game_state["penalties_paid"])

func track_money_earned(amount: int):
	add_progress("Milioner", amount)
