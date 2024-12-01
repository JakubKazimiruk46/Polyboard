extends Panel
@onready var game_settings = $"../GameSettings"

func _on_MainMenuButton_pressed():
	get_tree().change_scene_to_file("res://scenes/main.menu/main_menu.tscn")

func _on_ResumeButton_pressed():
	get_parent().visible = false

func _on_QuitButton_pressed():
	get_tree().quit()  # Wyjście z gry

func _on_OptionButton_pressed():
	self.visible = false
	game_settings.set_process(true)
	game_settings.visible = true

func on_exit_settings_menu():
	self.visible = true
	game_settings.visible = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	game_settings.exit_game_settings_menu.connect(on_exit_settings_menu)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
