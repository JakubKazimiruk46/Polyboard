extends Panel
@onready var game_settings = $"../GameSettings"
@onready var exit_mainmenu_confirm_window = $"../ExitMainMenuConfirm"
@onready var exit_game_confirm_window = $"../ExitGameConfirm"

func _on_MainMenuButton_pressed():
	self.visible = false
	exit_mainmenu_confirm_window.set_process(true)
	exit_mainmenu_confirm_window.visible = true

func _on_ResumeButton_pressed():
	get_parent().visible = false

func _on_QuitButton_pressed():
	self.visible = false
	exit_game_confirm_window.set_process(true)
	exit_game_confirm_window.visible = true

func _on_OptionButton_pressed():
	self.visible = false
	game_settings.set_process(true)
	game_settings.visible = true

func on_exit_settings_menu():
	self.visible = true
	game_settings.visible = false

func on_exit_confirm_menu():
	self.visible = true
	exit_mainmenu_confirm_window.visible = false

func on_exit_confirm_game():
	self.visible = true
	exit_game_confirm_window.visible = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	game_settings.exit_game_settings_menu.connect(on_exit_settings_menu)
	exit_mainmenu_confirm_window.exit_confirm_menu.connect(on_exit_confirm_menu)
	exit_game_confirm_window.exit_confirm_game.connect(on_exit_confirm_game)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
