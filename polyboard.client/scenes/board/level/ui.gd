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


func _ready() -> void:
	pass

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
		hide_owned_fields()
# Called every frame. 'delta' is the elapsed time since the previous frame.
func on_trade_button_pressed():
	trade.visible = true;

func on_build_button_pressed():
	var figurehead_script = preload("res://scenes/board/figurehead/Figurehead.cs")
	var board = $"../Board"
	var game_manager = $"../GameManager"
	var currentFigureHead = game_manager.getCurrentPlayer()
	var current_position = currentFigureHead.GetCurrentPositionIndex()
	var Field = game_manager.getCurrentField(current_position)
	var id = game_manager.GetCurrentPlayerIndex()
	if board and board.has_method("GetFieldById"):
		# Pobierz pole jako Node (lub Field, jeśli typowanie jest zaimplementowane)
		print(Field.FieldId)
		print(current_position)
		if Field and currentFigureHead == Field.Owner and Field.houseCost <= currentFigureHead.ECTS and Field.isHotel == false:
			currentFigureHead.ECTS -= Field.houseCost
			game_manager.UpdateECTSUI(id)
			
			Field.BuildingHouse(current_position)
		else:
			print("Nie znaleziono pola dla indeksu: %d / Pole nie należy do gracza" % current_position)
	#else:
		#print("Nie znaleziono instancji Board lub metoda GetFieldById nie istnieje!")

func on_remove_owner_button_pressed():
	print("REMOVE OWNER button pressed")
	var game_manager = $"../GameManager"
	var currentFigureHead = game_manager.getCurrentPlayer()
	var current_position = currentFigureHead.GetCurrentPositionIndex()
	var field = game_manager.getCurrentField(current_position)
	
	if field and field.owned:
		# Reset field ownership
		field.RemoveOwner()
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
	var figurehead_script = preload("res://scenes/board/figurehead/Figurehead.cs")
	var board = $"../Board"
	var game_manager = $"../GameManager"
	var currentFigureHead = game_manager.getCurrentPlayer() as Figurehead

	for i in range(card_hbox_container.get_child_count()):
		var texture_rect = card_hbox_container.get_child(i)
		if texture_rect is TextureRect:
			texture_rect.visible = false
	var displayed_index = 0  
	var owned_fields = currentFigureHead.GetOwnedFieldsAsArray()
	for i in range(owned_fields.size()):
		if owned_fields[i]:
			if displayed_index < card_hbox_container.get_child_count():
				var colorrect = card_hbox_container.get_child(displayed_index)
				var texture_rect=colorrect.get_child(0);
				if texture_rect is TextureRect:
					var texture_path = "res://scenes/board/level/textures/Field" + str(i) + ".png"
					var texture = load(texture_path)
					if texture:
						texture_rect.texture = texture
						texture_rect.visible = true
						displayed_index += 1  
					else:
						print("Nie udało się załadować tekstury: ", texture_path)
			else:
				print("Błąd: Brak wystarczającej liczby TextureRect w card_hbox_container!")
				break

func hide_owned_fields():
	for child in card_hbox_container.get_children():
		if child is ColorRect:
			child.visible = false

func _process(delta: float) -> void:
	pass


func _on_remove_owner_button_pressed() -> void:
	pass # Replace with function body.
