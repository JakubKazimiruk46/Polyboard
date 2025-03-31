extends Node2D

enum PopupIds{
	SHOW_LAST_MOUSE_POSITION = 100
}

var _last_mouse_position

@onready var _pm = $PopupMenu
@onready var _label = $Label

func _ready():
	_pm.add_item("Show last mouse position", PopupIds.SHOW_LAST_MOUSE_POSITION)
	_pm.id_pressed.connect(_on_PopupMenu_id_pressed)
	
func _input(event):
	if event is InputEventMouseButton and event.is_pressed() and event.button_index == MOUSE_BUTTON_RIGHT:
		_pm.position = _last_mouse_position
		_pm.popup()

func _on_PopupMenu_id_pressed(id: int) -> void:
	match id:
		PopupIds.SHOW_LAST_MOUSE_POSITION:
			print("DZIALA?")
			_label.text = "Last position: " + str(_last_mouse_position)

func _on_id_pressed(id: int) -> void:
	pass # Replace with function body.


func _on_index_pressed(index: int) -> void:
	pass # Replace with function body.
