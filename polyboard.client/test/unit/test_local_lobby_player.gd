extends GutTest

const PLAYER_SCENE := preload("res://scenes/local_lobby/local_lobby_player.tscn")
var player_ui

func before_each():
	player_ui = PLAYER_SCENE.instantiate()
	player_ui.player_id = 0
	player_ui.player_name_label_text = "Player1"
	add_child(player_ui)
	await get_tree().process_frame


func test_loads_label_text_correctly():
	assert_eq(player_ui.player_name_label.text, "Player1")


func test_open_nickname_dialog_sets_empty_text():
	player_ui.nickname_input.text = "TestNick"
	player_ui._on_set_nickname_pressed()
	assert_eq(player_ui.nickname_input.text, "")


func test_confirmed_nickname_updates_label_and_gamedata():
	var fake_name = "NowyNick"
	player_ui.nickname_input.text = fake_name
	player_ui._on_nickname_confirmed()
	assert_eq(player_ui.player_name_label.text, fake_name)
