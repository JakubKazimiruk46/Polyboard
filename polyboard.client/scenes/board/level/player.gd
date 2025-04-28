extends PanelContainer

signal panel_clicked(player_id)

@export var player_id: int = 0

func _gui_input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		panel_clicked.emit(player_id)
