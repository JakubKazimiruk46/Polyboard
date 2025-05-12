extends GutTest

var panel_scene = preload("res://scenes/board/level/game_menu.gd").new()

func before_each():
	panel_scene.game_settings = Control.new()
	panel_scene.game_settings.visible = false

	panel_scene.exit_mainmenu_confirm_window = Control.new()
	panel_scene.exit_mainmenu_confirm_window.visible = false

	panel_scene.exit_game_confirm_window = Control.new()
	panel_scene.exit_game_confirm_window.visible = false

	var parent = Control.new()
	parent.add_child(panel_scene)

func test_main_menu_button_pressed():
	panel_scene._on_MainMenuButton_pressed()
	assert_true(panel_scene.exit_mainmenu_confirm_window.visible, "Okno potwierdzenia powrotu do menu głównego powinno być widoczne.")

func test_resume_button_pressed():
	panel_scene.get_parent().visible = true

	panel_scene._on_ResumeButton_pressed()
	assert_false(panel_scene.get_parent().visible, "Menu powinno zostać ukryte.")

func test_quit_button_pressed():
	panel_scene._on_QuitButton_pressed()
	assert_true(panel_scene.exit_game_confirm_window.visible, "Okno potwierdzenia wyjścia z gry powinno być widoczne.")

func test_option_button_pressed():
	panel_scene._on_OptionButton_pressed()
	assert_true(panel_scene.game_settings.visible, "Okno ustawień powinno być widoczne.")
