extends CanvasLayer

@onready var trade = $"../Trade"
@onready var buycard = $"../BuyCard"
@onready var tradebutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button
@onready var buildbutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button
@onready var remove_owner_button = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer6/remove_owner_button
@onready var card_hbox_container = $Cards/ScrollContainer/MarginContainer/CardHBoxContainer
var cards_view = false
var buttons_view = false
const Figurehead=preload("res://scenes/board/figurehead/Figurehead.cs")
var game_manager = null
var board = null


func _ready() -> void:
	game_manager = $"../GameManager"
	board = $"../Board"


func on_cards_button_pressed():
	if cards_view == false:
		cards_view = true
		var target_anchor_top = 0.85
		var target_anchor_bottom = 1
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Cards, "anchor_top", target_anchor_top, 0.5)
		tween.tween_property($Cards, "anchor_bottom", target_anchor_bottom, 0.5)
		tween.set_parallel(false)
		display_owned_fields()
	else:
		var target_anchor_top = 1.05
		var target_anchor_bottom = 1.2
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Cards, "anchor_top", target_anchor_top, 0.5)
		tween.tween_property($Cards, "anchor_bottom", target_anchor_bottom, 0.5)
		tween.set_parallel(false)
		cards_view = false

func on_trade_button_pressed():
	trade.visible = true

func on_build_button_pressed():
	var figurehead_script = preload("res://scenes/board/figurehead/Figurehead.cs")
	var currentFigureHead = game_manager.getCurrentPlayer()
	var current_position = currentFigureHead.GetCurrentPositionIndex()
	var Field = game_manager.getCurrentField(current_position)
	var id = game_manager.GetCurrentPlayerIndex()
	if board and board.has_method("GetFieldById"):
		print(Field.FieldId)
		print(current_position)
		if Field and currentFigureHead == Field.Owner and Field.houseCost <= currentFigureHead.ECTS and Field.isHotel == false:
			currentFigureHead.ECTS -= Field.houseCost
			game_manager.UpdateECTSUI(id)
			Field.BuildingHouse(current_position)
		else:
			print("Nie znaleziono pola dla indeksu: %d / Pole nie należy do gracza" % current_position)

func on_remove_owner_button_pressed():
	print("REMOVE OWNER button pressed")
	var currentFigureHead = game_manager.getCurrentPlayer()
	var current_position = currentFigureHead.GetCurrentPositionIndex()
	var field = game_manager.getCurrentField(current_position)
	
	if field and field.owned:
		field.RemoveOwner(currentFigureHead, field)
		print("Field ownership removed from: " + field.Name)
	else:
		print("Cannot remove ownership: field is not owned")

func on_view_buttons_pressed():
	if buttons_view == false:
		buttons_view = true
		var target_anchor_left = 1
		var target_anchor_right = 1
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($HBoxContainer, "anchor_left", target_anchor_left, 0.5)
		tween.tween_property($HBoxContainer, "anchor_right", target_anchor_right, 0.5)
		tween.set_parallel(false)
	else:
		var target_anchor_left = 0.95
		var target_anchor_right = 1.14
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($HBoxContainer, "anchor_left", target_anchor_left, 0.5)
		tween.tween_property($HBoxContainer, "anchor_right", target_anchor_right, 0.5)
		tween.set_parallel(false)
		buttons_view = false

func display_owned_fields():
	var currentFigureHead = game_manager.getCurrentPlayer() as Figurehead

	for child in card_hbox_container.get_children():
		child.visible = false
		if child.get_child_count() > 0 and child.get_child(0) is TextureRect:
			child.get_child(0).visible = false
			if child.get_child(0).is_connected("mouse_entered", Callable(self, "_on_card_mouse_entered")):
				child.get_child(0).disconnect("mouse_entered", Callable(self, "_on_card_mouse_entered"))
			if child.get_child(0).is_connected("mouse_exited", Callable(self, "_on_card_mouse_exited")):
				child.get_child(0).disconnect("mouse_exited", Callable(self, "_on_card_mouse_exited"))

	var owned_fields = currentFigureHead.GetOwnedFieldsAsArray()
	var has_any_cards = false

	for i in range(owned_fields.size()):
		if owned_fields[i]:
			has_any_cards = true
			break

	if not has_any_cards:
		return

	var displayed_index = 0
	for i in range(owned_fields.size()):
		if owned_fields[i] and displayed_index < card_hbox_container.get_child_count():
			var colorrect = card_hbox_container.get_child(displayed_index)
			colorrect.visible = true
			
			if colorrect.get_child_count() > 0 and colorrect.get_child(0) is TextureRect:
				var texture_rect = colorrect.get_child(0)
				var texture_path = "res://scenes/board/level/textures/Field" + str(i) + ".png"
				var texture = load(texture_path)
				
				if texture:
					texture_rect.texture = texture
					texture_rect.visible = true
					texture_rect.connect("mouse_entered", Callable(self, "_on_card_mouse_entered").bind(i))
					texture_rect.connect("mouse_exited", Callable(self, "_on_card_mouse_exited"))
					displayed_index += 1
				else:
					colorrect.visible = false

func _on_card_mouse_entered(field_id: int):
	if board and board.has_method("ShowFieldTexture"):
		board.ShowFieldTexture(field_id)

func _on_card_mouse_exited():
	if board and board.has_method("HideFieldTexture"):
		board.HideFieldTexture()

func _process(delta: float) -> void:
	pass
