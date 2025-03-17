extends CanvasLayer

@onready var trade = $"../Trade"
@onready var buycard = $"../BuyCard"
@onready var tradebutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button
@onready var buildbutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button
var cards_view = false
var buttons_view = false

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
	else:
		var target_anchor_top = 1.05
		var target_anchor_bottom = 1.2
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Cards, "anchor_top", target_anchor_top, 0.5)
		tween.tween_property($Cards, "anchor_bottom", target_anchor_bottom, 0.5)
		tween.set_parallel(false)
		cards_view = false
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

func _process(delta: float) -> void:
	pass
