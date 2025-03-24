extends GutTest

var game_data = preload("res://services/game_data.gd").new()

func before_each():
	game_data.reset_data()

# Sprawdzanie domyślnej liczby graczy
func test_default_players():
	assert_eq(game_data.get_player_count(), game_data.MIN_PLAYERS, "Powinna być minimalna liczba graczy")

# Dodanie gracza zwiększa liczbę graczy
func test_add_player():
	game_data.add_player()
	assert_eq(game_data.get_player_count(), game_data.MIN_PLAYERS + 1, "Liczba graczy powinna wzrosnąć o 1")

# Usunięcie gracza zmniejsza liczbę graczy
func test_remove_player():
	var initial_count = game_data.get_player_count()

	game_data.remove_player()

	# Liczba graczy nie powinna spaść poniżej MIN_PLAYERS
	var expected_count = max(initial_count - 1, game_data.MIN_PLAYERS)
	assert_eq(game_data.get_player_count(), expected_count, "Liczba graczy nie powinna być mniejsza niż MIN_PLAYERS")

# Sprawdzenie zmiany nicku
func test_set_player_name():
	game_data.set_player_name(0, "NowyNick")
	assert_eq(game_data.get_player_name_by_id(0), "NowyNick", "Nick pierwszego gracza powinien się zmienić")

# Sprawdzenie zmiany skina
func test_set_player_skin():
	game_data.set_player_skin(2)
	assert_eq(game_data.players[0]["skin"], 2, "Skin pierwszego gracza powinien się zmienić")

# Sprawdzenie aktualnego edytowanego gracza
func test_set_current_player_editing_skin():
	game_data.set_current_player_editing_skin(1)
	assert_eq(game_data.get_current_player_editing_skin(), 1, "Id gracza edytującego skin powinno się zmienić")
