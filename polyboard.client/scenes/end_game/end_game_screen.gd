extends CanvasLayer

@onready var results_container = $Panel/VBoxContainer/ResultsContainer
@onready var return_button = $Panel/VBoxContainer/ReturnToMenuButton

func _ready():
	return_button.pressed.connect(_on_return_pressed)

func _on_return_pressed():
	get_tree().change_scene_to_file("res://scenes/main.menu/main_menu.tscn")

func set_results(result_message: String, player_results: Array):
	# Wyczyść istniejące wyniki
	for child in results_container.get_children():
		child.queue_free()
	
	# Dodaj główny komunikat
	var result_label = Label.new()
	result_label.text = result_message
	result_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	result_label.add_theme_font_size_override("font_size", 24)
	results_container.add_child(result_label)
	
	# Dodaj separator
	var separator = HSeparator.new()
	separator.custom_minimum_size = Vector2(300, 10)
	separator.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	results_container.add_child(separator)
	
	# Dodaj nagłówek tabeli wyników
	var header_label = Label.new()
	header_label.text = "WYNIKI KOŃCOWE:"
	header_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	header_label.add_theme_font_size_override("font_size", 18)
	results_container.add_child(header_label)
	
	# Dodaj wyniki poszczególnych graczy
	for result in player_results:
		var player_container = HBoxContainer.new()
		player_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		player_container.alignment = BoxContainer.ALIGNMENT_CENTER
		
		var name_label = Label.new()
		name_label.text = result.name
		name_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		var status_label = Label.new()
		status_label.text = "BANKRUT" if result.bankrupt else str(result.ects) + " ECTS"
		status_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		if result.bankrupt:
			name_label.add_theme_color_override("font_color", Color(1, 0, 0))
			status_label.add_theme_color_override("font_color", Color(1, 0, 0))
		
		player_container.add_child(name_label)
		player_container.add_child(status_label)
		
		results_container.add_child(player_container)
