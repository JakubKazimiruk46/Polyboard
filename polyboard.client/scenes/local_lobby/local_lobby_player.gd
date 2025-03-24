extends Control

@onready var player_name_label = $PlayerContainer/PlayerName
@onready var set_nickname_button = $PlayerContainer/SetNicknameButton
@onready var nickname_dialog = $PlayerContainer/NicknameDialog
@onready var nickname_input = $PlayerContainer/NicknameDialog/NicknameInput
@onready var set_skin_button = $PlayerContainer/SetSkinButton

var player_id = -1
var player_name_label_text = ""

func _ready():
	_load_player_name_label_text()
	set_skin_button.pressed.connect(_on_set_skin_pressed)
	set_nickname_button.pressed.connect(_on_set_nickname_pressed)
	nickname_dialog.confirmed.connect(_on_nickname_confirmed)
	nickname_dialog.title = "Wpisz nowy nick!"
	nickname_dialog.size = Vector2(300, 120)

func _load_player_name_label_text():
	player_name_label.text = player_name_label_text

func _on_set_skin_pressed():
	GameData.set_current_player_editing_skin(player_id)
	
	get_tree().change_scene_to_file("res://scenes/set_skins/set_skins.tscn")

func _on_set_nickname_pressed():
	nickname_input.text = ""
	nickname_dialog.popup_centered()

func _on_nickname_confirmed():
	var new_name = nickname_input.text.strip_edges()
	if new_name != "":
		player_name_label.text = new_name
		GameData.set_player_name(player_id, new_name)
		GameData.print_player_list()
