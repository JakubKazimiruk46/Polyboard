extends Panel


# Called when the node enters the scene tree for the first time.
@onready var settings_tab_container = $MarginContainer/settings_tab_container as SettingsTabContainer

signal exit_game_settings_menu

func _ready():
	settings_tab_container.Exit_Options_menu.connect(on_back_button_pressed)
	set_process(false)
	
func on_back_button_pressed() -> void:
	exit_game_settings_menu.emit()
	SettingsSignalBus.emit_set_settings_dictionary(SettingsDataContainer.create_storage_dictionary())
	set_process(false)
