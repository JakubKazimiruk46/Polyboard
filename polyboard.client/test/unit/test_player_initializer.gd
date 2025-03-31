extends GutTest

var player_initializer

# 🔹 Tworzymy stub GameData
class GameDataStub:
	func get_player_count():
		return 3
	
	func get_player_skin_id_by_id(id):
		return id
	
	func get_player_name_by_id(id):
		return "Player" + str(id)

# 🔹 Tworzymy stub SkinManager
class SkinManagerStub:
	func get_skin_by_index(id):
		var packed_scene = PackedScene.new()
		var player_instance = Node3D.new()
		packed_scene.pack(player_instance)
		return packed_scene

func before_each():
	# 🔹 Tworzymy instancję PlayerInitializer i dodajemy do sceny
	player_initializer = load("res://scenes/board/level/PlayerInitializer.tscn").instantiate()
	add_child(player_initializer)
 
	# 🔹 Dodajemy stuby do sceny jako autoloady
	get_tree().set_meta("GameData", GameDataStub.new())
	get_tree().set_meta("SkinManager", SkinManagerStub.new())

func after_each():
	# 🔹 Usuwamy instancję po każdym teście
	player_initializer.queue_free()

func test_create_player():
	# 🔹 Tworzymy gracza
	var player = player_initializer.call("create_player", 0)

	# 🔹 Sprawdzamy poprawność utworzonego gracza
	assert_not_null(player, "Gracz nie został utworzony poprawnie")
	assert_eq(player.name, "Player0", "Gracz ma niepoprawną nazwę")

	# 🔹 Sprawdzamy, czy gracz ma etykietę `Label3D`
	assert_eq(player.get_child_count(), 1, "Gracz powinien mieć jedno dziecko (Label3D)")
	assert_eq(player.get_child(0).text, "Player0", "Label3D nie ma poprawnego tekstu")
