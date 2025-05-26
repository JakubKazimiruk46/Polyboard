extends GutTest

const LOGIN_SCENE_PATH = "res://scenes/login/login.tscn"

var login_scene
var login_instance

func before_each():
	login_scene = load(LOGIN_SCENE_PATH)
	login_instance = login_scene.instantiate()
	add_child(login_instance)

	await get_tree().process_frame

func after_each():
	login_instance.queue_free()
	await get_tree().process_frame

func test_buttons_are_connected():
	assert_true(login_instance.register_button.pressed.is_connected(login_instance.on_register_pressed), "Register button niepodłączony")
	assert_true(login_instance.login_button.pressed.is_connected(login_instance.on_login_pressed), "Login button niepodłączony")
	assert_true(login_instance.back_button.pressed.is_connected(login_instance.on_back_button_pressed), "Back button niepodłączony")
	assert_true(login_instance.register_menu.exit_register_menu.is_connected(login_instance.on_exit_register_menu), "Signal exit_register_menu niepodłączony")

func test_empty_username_shows_error():
	login_instance.username_input.text = ""
	login_instance.password_input.text = "password123"
	login_instance.on_login_pressed()
	await get_tree().process_frame
	assert_eq(login_instance.error_label.text, "Username cannot be empty.", "Brak błędu przy pustym loginie")

func test_empty_password_shows_error():
	login_instance.username_input.text = "TestUser"
	login_instance.password_input.text = ""
	login_instance.on_login_pressed()
	await get_tree().process_frame
	assert_eq(login_instance.error_label.text, "Password cannot be empty.", "Brak błędu przy pustym haśle")

func test_click_sounds_are_played():
	login_instance.click_sound.stop()
	login_instance.on_register_pressed()
	assert_true(login_instance.click_sound.playing, "Nie zagrano dźwięku po kliknięciu przycisku")

func test_register_menu_visibility_toggle():
	login_instance.on_register_pressed()
	await get_tree().process_frame
	assert_false(login_instance.margin_container.visible, "Margin container powinien być ukryty")
	assert_true(login_instance.register_menu.visible, "Register menu powinien być widoczny")

	login_instance.on_exit_register_menu()
	await get_tree().process_frame
	assert_true(login_instance.margin_container.visible, "Margin container powinien być widoczny")
	assert_false(login_instance.register_menu.visible, "Register menu powinien być ukryty")
