extends GutTest

const ACCOUNT_SCENE_PATH = "res://scenes/account/account.tscn"

var account_scene
var account_instance

func before_each():
	account_scene = load(ACCOUNT_SCENE_PATH)
	account_instance = account_scene.instantiate()
	add_child(account_instance)
	await get_tree().process_frame

func after_each():
	account_instance.queue_free()
	await get_tree().process_frame


func test_buttons_are_connected():
	assert_true(account_instance.save_button.pressed.is_connected(account_instance._on_save_pressed), "Przycisk zapisu niepodłączony")
	assert_true(account_instance.back_button.pressed.is_connected(account_instance.on_back_button_pressed), "Przycisk powrotu niepodłączony")
	assert_true(account_instance.http_request.request_completed.is_connected(account_instance._on_edit_request_completed), "Sygnał HTTP niepodłączony")


func test_password_mismatch_error():
	account_instance.new_pass.text = "haslo1"
	account_instance.confirm_new_pass.text = "haslo2"
	account_instance._on_save_pressed()
	await get_tree().process_frame
	assert_eq(account_instance.error_label.text, "Hasla sa niezgodne!", "Brak błędu przy niezgodnych hasłach")

func test_successful_response_sets_success_label():
	var body := PackedByteArray()
	body.append_array("OK".to_utf8_buffer())
	account_instance._on_edit_request_completed(0, 200, [], body)
	await get_tree().process_frame
	assert_eq(account_instance.error_label.text, "Profile updated successfully.", "Brak komunikatu o sukcesie")


func test_error_response_sets_error_label():
	var body := PackedByteArray()
	body.append_array("Invalid password".to_utf8_buffer())
	account_instance._on_edit_request_completed(0, 400, [], body)
	await get_tree().process_frame
	assert_eq(account_instance.error_label.text, "Error: Invalid password", "Błędna odpowiedź nie ustawiła poprawnego komunikatu")


func test_click_sound_is_played_on_save():
	account_instance.click_sound.stop()
	account_instance._on_save_pressed()
	assert_true(account_instance.click_sound.playing, "Nie zagrano dźwięku przy zapisie")


func test_click_sound_is_played_on_back():
	account_instance.click_sound.stop()
	account_instance.on_back_button_pressed()
	assert_true(account_instance.click_sound.playing, "Nie zagrano dźwięku przy powrocie")
