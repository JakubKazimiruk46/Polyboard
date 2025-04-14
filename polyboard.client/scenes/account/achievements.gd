extends Control
class_name Achievements

@onready var tab_container = $".."
@onready var vbox_container = $MarginContainer/VBoxContainer/ScrollContainer/VBoxContainer
@onready var filter = $MarginContainer/VBoxContainer/AchievementFilter
@onready var AchievementManager = $AchievementManager
@onready var save_progress = $SaveProgressButton
var current_filter: String = "All"

func _ready() -> void:
	tab_container.current_tab = 0
	filter.connect("filter_changed", _on_filter_changed)
	save_progress.connect("pressed", _save_progress)

func _process(_delta):
	if not self.visible:
		clear_achievements()
func _save_progress() -> void:
	AchievementManager.save_progress()
func _on_filter_changed(new_filter: String) -> void:
	set_filter(new_filter)
	clear_achievements()
	load_achievements()

func _visibility_changed() -> void:
	if self.visible:
		load_achievements()

func set_filter(new_filter: String) -> void:
	current_filter = new_filter

func load_achievements() -> void:
	print("Loading achievements from AchievementManager")
	var achievements = AchievementManager.get_achievements()
	display_user_achievements(achievements)

func clear_achievements():
	for child in vbox_container.get_children():
		vbox_container.remove_child(child)
		child.queue_free()

func display_user_achievements(achievements: Array) -> void:
	for achievement in achievements:
		if current_filter == "Achieved" and achievement.get("progress", 0.0) < 100.0:
			continue
		elif current_filter == "Not achieved" and achievement.get("progress", 0.0) >= 100.0:
			continue

		var hbox = HBoxContainer.new()
		var texture_rect = TextureRect.new()
		var name_label = Label.new()
		var progress_label = Label.new()
		var description_label = Label.new()

		var ach = achievement.achievement
		var name = ach.get("name", "Unknown")
		var requirement = ach.get("requirement", "Brak opisu")
		var progress = achievement.get("progress", 0.0)

		# przypisanie obrazków
		match name:
			"Finansowy mistrz": texture_rect.texture = load("res://assets/images/taxes1.png")
			"Budowniczy imperium": texture_rect.texture = load("res://assets/images/house.png")
			"Strategiczny profesor": texture_rect.texture = load("res://assets/images/dollar.png")
			"Magnat inwestycyjny": texture_rect.texture = load("res://assets/images/apartment.png")
			"Bezlitosny": texture_rect.texture = load("res://assets/images/bankruptcy.png")
			"Milioner": texture_rect.texture = load("res://assets/images/coin.png")
			"Pierwszy krok": texture_rect.texture = load("res://assets/images/success.png")
			"Dziekan wydziału inwestycji": texture_rect.texture = load("res://assets/images/architect.png")
			"Dubletowy szczęściarz": texture_rect.texture = load("res://assets/images/dice-game.png")
			"Akademicki inwestor": texture_rect.texture = load("res://assets/images/investor.png")
			"Ekspert ds. dywersyfikacji": texture_rect.texture = load("res://assets/images/property.png")
			"Podatnik Roku": texture_rect.texture = load("res://assets/images/taxes.png")
			"Prześladowany przez los": texture_rect.texture = load("res://assets/images/jail.png")

		name_label.text = name
		progress_label.text = str(round(progress)) + "%"
		description_label.text = requirement

		name_label.custom_minimum_size = Vector2(220, 0)
		progress_label.custom_minimum_size = Vector2(40, 0)
		description_label.custom_minimum_size = Vector2(400, 0)

		var spacer_1 = Control.new(); spacer_1.custom_minimum_size = Vector2(20, 0)
		var spacer_2 = Control.new(); spacer_2.custom_minimum_size = Vector2(20, 0)
		var spacer_3 = Control.new(); spacer_3.custom_minimum_size = Vector2(20, 0)

		hbox.add_child(texture_rect)
		hbox.add_child(spacer_1)
		hbox.add_child(name_label)
		hbox.add_child(spacer_2)
		hbox.add_child(progress_label)
		hbox.add_child(spacer_3)
		hbox.add_child(description_label)

		vbox_container.add_child(hbox)
