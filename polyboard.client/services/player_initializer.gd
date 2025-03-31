extends Node

@export var player_container_path: NodePath
var players_container: Node = null

func _ready():
	players_container = get_node_or_null(player_container_path)
	if players_container == null:
		printerr("Nie znaleziono węzła pod ścieżką playersContainerPath")
		return

func initialize_players():
	var player_count = GameData.get_player_count()
	var players_loaded = 0

	print("Rozpoczynam inicjalizację graczy... Liczba graczy w GameData:", player_count)

	for i in range(player_count):
		print("Tworzę gracza", i)
		print(player_container_path)
		var player = create_player(i)
		if player:
			print("Przed dodaniem gracza do playersContainer: ", players_container, "Children:", players_container.get_child_count())
			players_container.add_child(player)
			print("Po dodaniu gracza: ", players_container.get_child_count())
			players_loaded += 1
		else:
			print("Nie udało się dodać gracza o ID:", i)

	print("Załadowano %d graczy. Liczba graczy w gamedata: %d" % [players_loaded, player_count])


func create_player(player_id):
	print("Inicjalizacja gracza ID:", player_id)
	var skin_id = GameData.get_player_skin_id_by_id(player_id)
	var player_name = GameData.get_player_name_by_id(player_id)

	var skin_manager = get_node("/root/SkinManager")
	if skin_manager == null:
		printerr("Nie znaleziono SkinManager!")
		return null

	var skin_scene = skin_manager.get_skin_by_index(skin_id)
	if skin_scene == null:
		printerr("Błąd podczas pobierania sceny skina dla gracza %s (ID: %d)" % [player_name, skin_id])
		return null

	var player_instance = skin_scene.instantiate()
	if player_instance == null:
		printerr("Błąd podczas instancjacji pionka dla gracza %s" % player_name)
		return null

	player_instance.name = player_name
	
	var label = Label3D.new()
	label.text = player_name
	label.position = Vector3(0, 5, 0) 
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED  
	label.modulate = Color(1, 1, 1)  
	label.font_size = 600  
	
	player_instance.add_child(label)
	
	print("Utworzono pionek dla gracza:", player_name)

	return player_instance
