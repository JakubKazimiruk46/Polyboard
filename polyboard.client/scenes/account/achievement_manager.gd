extends Node
class_name AchievementManager

const ACHIEVEMENTS_PATH = "res://data/achievements.json"
const PROGRESS_PATH = "res://data/progress.json"

var achievements: Dictionary = {} 
var progress_data: Dictionary = {} 

func _ready():
	load_achievements()
	load_progress()

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

func load_progress():
	if FileAccess.file_exists(PROGRESS_PATH):
		var file = FileAccess.open(PROGRESS_PATH, FileAccess.READ)
		var content = file.get_as_text()
		var parsed = JSON.parse_string(content)
		if parsed:
			progress_data = parsed
		else:
			print("Błąd parsowania progress.json")
	else:
		# Inicjalizacja nowego pliku
		for name in achievements.keys():
			progress_data[name] = 0.0
		save_progress()

func save_progress():
	var file = FileAccess.open(PROGRESS_PATH, FileAccess.WRITE)
	file.store_string(JSON.stringify(progress_data, "\t"))
	file.close()
	print("Zapisano postęp!")

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
			"raw_progress": raw_progress
		})

	return result

func add_progress(name: String, value: float):
	if not achievements.has(name):
		print("Nie znaleziono osiągnięcia:", name)
		return

	var current = progress_data.get(name, 0.0)
	progress_data[name] = current + value
	save_progress()
