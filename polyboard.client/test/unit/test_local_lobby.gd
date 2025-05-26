extends GutTest

const LOBBY_SCENE_PATH = "res://scenes/local_lobby/local_lobby.tscn"

var lobby_scene
var lobby_instance

func before_each():
	lobby_scene = load(LOBBY_SCENE_PATH)
	lobby_instance = lobby_scene.instantiate()
	add_child(lobby_instance)

	await get_tree().process_frame

func after_each():
	lobby_instance.queue_free()
	await get_tree().process_frame


func test_buttons_are_connected():
	assert_true(lobby_instance.add_player_button.pressed.is_connected(lobby_instance._on_add_player_pressed), "Add button niepodłączony")
	assert_true(lobby_instance.remove_player_button.pressed.is_connected(lobby_instance._on_remove_player_pressed), "Remove button niepodłączony")
	assert_true(lobby_instance.start_button.pressed.is_connected(lobby_instance._on_start_button_pressed), "Start button niepodłączony")


func test_add_and_remove_player_changes_player_count():
	var initial_count = GameData.get_player_count()
	
	# Dodaj gracza
	lobby_instance._on_add_player_pressed()
	var new_count = GameData.get_player_count()
	assert_eq(new_count, initial_count + 1, "Gracz nie został dodany")
	
	# Usuń gracza
	lobby_instance._on_remove_player_pressed()
	var final_count = GameData.get_player_count()
	assert_eq(final_count, initial_count, "Gracz nie został usunięty")

func test_add_button_disabled_on_max_players():
	# Wymuś maksymalną liczbę graczy
	while GameData.get_player_count() < GameData.MAX_PLAYERS:
		lobby_instance._on_add_player_pressed()

	assert_true(lobby_instance.add_player_button.disabled, "Przycisk dodawania graczy powinien być zablokowany")

func test_remove_button_disabled_on_min_players():
	# Usuń graczy aż do minimum
	while GameData.get_player_count() > GameData.MIN_PLAYERS:
		lobby_instance._on_remove_player_pressed()

	assert_true(lobby_instance.remove_player_button.disabled, "Przycisk usuwania graczy powinien być zablokowany")
