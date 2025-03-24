extends Node

var skins = [
	preload("res://scenes/board/figurehead/pizza_pionek.tscn"),   
	preload("res://scenes/board/figurehead/xbox_controller_pionek.tscn"),
	preload("res://scenes/board/figurehead/czapka_absolwenta_pionek.tscn"),
	preload("res://scenes/board/figurehead/lampka_pionek.tscn"),
	preload("res://scenes/board/figurehead/mikroskop_pionek.tscn"),
	preload("res://scenes/board/figurehead/lego_pionek.tscn")
]

func get_skins() -> Array:
	return skins

func get_skin_by_index(index: int):
	if index >= 0 and index < skins.size():
		return skins[index]
	return null
