extends Control

@onready var player_name_label = $PlayerContainer/PlayerName
@onready var set_nickname_button = $PlayerContainer/SetNicknameButton
@onready var nickname_dialog = $PlayerContainer/NicknameDialog
@onready var nickname_input = $PlayerContainer/NicknameDialog/NicknameInput

func _ready():
	set_nickname_button.pressed.connect(_on_set_nickname_pressed)
	nickname_dialog.confirmed.connect(_on_nickname_confirmed)
	nickname_dialog.title = "Wpisz nowy nick!"
	nickname_dialog.size = Vector2(300, 120)

func _on_set_nickname_pressed():
	nickname_input.text = ""
	nickname_dialog.popup_centered()

func _on_nickname_confirmed():
	var new_name = nickname_input.text.strip_edges()
	if new_name != "":
		player_name_label.text = new_name
