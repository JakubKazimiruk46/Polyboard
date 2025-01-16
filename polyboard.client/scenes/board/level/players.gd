extends Node3D

func _ready():
	for player in get_children():
		if player is CharacterBody3D:
			var label = player.get_node("Label3D") as Label3D
			label.text = player.name
			label.font_size = 256
			label.translate(Vector3(0,3,0))
		
