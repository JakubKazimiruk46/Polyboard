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
	var skin_id = GameData.get_player_skin_id_by_id(player_id)
	var player_name = GameData.get_player_name_by_id(player_id)
	
	var skin_manager = get_node("/root/SkinManager")
	var skin_scene = skin_manager.get_skin_by_index(skin_id)
	var player_instance = skin_scene.instantiate()
	player_instance.name = player_name
	
	# Znajdź najwyższy punkt modelu
	var max_height = find_highest_point(player_instance)
	print("Najwyższy punkt pionka: ", max_height)
	var parent_scale = player_instance.scale
	var label = create_player_label(player_name, max_height, parent_scale, player_instance)
	player_instance.add_child(label)
	
	return player_instance

func find_highest_point(node: Node) -> float:
	var max_y = -INF
	for child in node.get_children():
		if child is MeshInstance3D:
			var aabb = child.get_aabb()
		
			max_y = max(max_y, aabb.position.y + aabb.size.y)
		if child.get_child_count() > 0:
			max_y = max(max_y, find_highest_point(child))
		
	return max_y

func create_player_label(name: String, height: float,parent_scale:Vector3,parent_node:Node3D) -> Label3D:
	var label = Label3D.new()
	label.text = name
	var model_up = parent_node.global_transform.basis.y
	var model_forward = parent_node.global_transform.basis.z
	
	var needs_correction = abs(model_up.dot(Vector3.UP)) < 0.9
	var base_offset = Vector3(0, height + 0.5, 0)
	
	if needs_correction:
		var correction = model_forward * -0.5 * height 
		base_offset += correction
	
	
	label.position = base_offset
	
	label.scale = Vector3(0.1 / parent_scale.x, 0.1 / parent_scale.y, 0.1 / parent_scale.z)
	label.billboard = BaseMaterial3D.BILLBOARD_FIXED_Y
	label.modulate = Color(1, 1, 1)
	label.font_size = 600
	return label
