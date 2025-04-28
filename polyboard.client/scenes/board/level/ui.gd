extends CanvasLayer

@onready var trade = $"../Trade"
@onready var buycard = $"../BuyCard"
@onready var tradebutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button
@onready var buildbutton = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button
@onready var card_hbox_container = $Cards/ScrollContainer/MarginContainer/CardHBoxContainer
@onready var special_cards = $SpecialCards
@onready var special_card_button = $HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer5/special_card_button
@onready var player_stats_panel = $PlayerStatsPanelContainer
@onready var exit_stats_button = $PlayerStatsPanelContainer/PlayerStatsPanel/PlayerStats/ExitButton
@onready var loan_button=$HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer7/laon_button

var cards_view = false
var buttons_view = false
const Figurehead=preload("res://scenes/board/figurehead/Figurehead.cs")
var game_manager = null
var board = null
const LoanPopupScene = preload("res://scenes/board/level/loan_popup.tscn")
const LoanPayPopupScene=preload("res://scenes/board/level/loan_pay_popup.tscn")




func _ready() -> void:
	game_manager = $"../GameManager"
	board = $"../Board"
	special_cards.visible = false
	apply_style_to_stats_panel()
	
	exit_stats_button.pressed.connect(_on_exit_stats_button)
	
	for panel in $Players.get_children():
		if panel is PanelContainer and panel.has_signal("panel_clicked"):
			print("istnieje taki panel")
			panel.panel_clicked.connect(_on_panel_clicked)

func _on_exit_stats_button():
	player_stats_panel.visible = false

func _on_panel_clicked(player_id: int):
	var player_name = GameData.get_player_name_by_id(player_id)
	if not player_name:
		print("Nie znaleziono gracza o ID:", player_id)
		return
		
	player_stats_panel.visible = true
	var stats_panel = $PlayerStatsPanelContainer/PlayerStatsPanel/PlayerStats
	
	var players = game_manager.GetPlayers()
	if player_id < 0 or player_id >= players.size():
		printerr("Invalid player ID: ", player_id)
		return
	
	var player = players[player_id]
	if not player:
		printerr("Player not found!")
		return
	
	var name_label = stats_panel.get_node("Playername")
	var cash_label = stats_panel.get_node("Cash/Value")
	var properties_label = stats_panel.get_node("Properties/Value")
	var properties_list_label = stats_panel.get_node("PropertiesList/Value")
	
	var owned_fields = player.GetOwnedFields()
	var field_names = []
	for field in board.GetFieldsOwnedByPlayerName(player_name):
		field_names.push_back(field["name"])
	
	name_label.text = player_name
	cash_label.text = str(player.ECTS)
	properties_label.text = str(owned_fields.size())
	properties_list_label.text = ", ".join(field_names)
	
func apply_style_to_stats_panel():
	var panel = $PlayerStatsPanelContainer/PlayerStatsPanel

	var panel_style = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.15, 0.15, 0.18, 0.85)
	panel_style.corner_radius_top_left = 12
	panel_style.corner_radius_top_right = 12
	panel_style.corner_radius_bottom_left = 12
	panel_style.corner_radius_bottom_right = 12
	panel_style.border_width_top = 1
	panel_style.border_width_bottom = 1
	panel_style.border_width_left = 1
	panel_style.border_width_right = 1
	panel_style.border_color = Color(0.4, 0.8, 0.6, 0.5) 
	
	panel.add_theme_stylebox_override("panel", panel_style)

	var font = preload("res://assets/fonts/PlayerStatsFont.tres") 
	var text_color = Color(0.9, 0.9, 0.9) 

	for container in $PlayerStatsPanelContainer/PlayerStatsPanel/PlayerStats.get_children():
		for label in container.get_children():
			if label is RichTextLabel:
				label.add_theme_color_override("default_color", text_color)
				label.add_theme_font_override("normal_font", font)
				label.add_theme_font_size_override("normal_font_size", 18)

	var button = $PlayerStatsPanelContainer/PlayerStatsPanel/PlayerStats/ExitButton
	button.add_theme_color_override("font_color", text_color)
	button.add_theme_color_override("font_color_hover", Color(0.95, 1.0, 0.95))
	button.add_theme_color_override("font_color_pressed", Color(0.85, 1.0, 0.85))
	button.add_theme_font_override("font", font)
	button.add_theme_font_size_override("font_size", 16)

	var button_style_normal = StyleBoxFlat.new()
	button_style_normal.bg_color = Color(0.25, 0.85, 0.55)
	button_style_normal.corner_radius_top_left = 10
	button_style_normal.corner_radius_top_right = 10
	button_style_normal.corner_radius_bottom_left = 10
	button_style_normal.corner_radius_bottom_right = 10
	button_style_normal.border_width_top = 1
	button_style_normal.border_width_bottom = 1
	button_style_normal.border_width_left = 1
	button_style_normal.border_width_right = 1
	button_style_normal.border_color = Color(1, 1, 1, 0.3)

	var button_style_hover = button_style_normal.duplicate()
	button_style_hover.bg_color = Color(0.3, 0.9, 0.6) 

	var button_style_pressed = button_style_normal.duplicate()
	button_style_pressed.bg_color = Color(0.2, 0.7, 0.5) 

	button.add_theme_stylebox_override("normal", button_style_normal)
	button.add_theme_stylebox_override("hover", button_style_hover)
	button.add_theme_stylebox_override("pressed", button_style_pressed)

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
			currentFigureHead.SpendECTS(Field.houseCost)
			game_manager.UpdateECTSUI(id)
			Field.BuildingHouse(current_position)
		else:
			print("Nie znaleziono pola dla indeksu: %d / Pole nie należy do gracza" % current_position)



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
					
func _gui_input(event):
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		pass
	
func _on_card_mouse_entered(field_id: int):
	if board and board.has_method("ShowFieldTexture"):
		board.ShowFieldTexture(field_id)

func _on_card_mouse_exited():
	if board and board.has_method("HideFieldTexture"):
		board.HideFieldTexture()

func _process(delta: float) -> void:
	pass

func _on_special_card_button_pressed():
	if not special_cards.visible:
		special_cards.visible = true

		var target_anchor_top = 0.85
		var target_anchor_bottom = 1.0

		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property(special_cards, "anchor_top", target_anchor_top, 0.5)
		tween.tween_property(special_cards, "anchor_bottom", target_anchor_bottom, 0.5)
		tween.set_parallel(false)

		# Pokaż 2 konkretne karty
		var card_container = special_cards.get_node("ScrollContainer/MarginContainer/CardHBoxContainer")

		var textures = [
			"res://scenes/board/level/textures/chances/chance_14.png",
			"res://scenes/board/level/textures/community_chest/community_14.png"
		]

		for i in range(min(card_container.get_child_count(), 2)):
			var color_rect = card_container.get_child(i)
			color_rect.visible = true
			color_rect.custom_minimum_size = Vector2(200, 300)  # ✅ Ustaw rozmiar karty

			# Usuń poprzedni TextureRect, jeśli istnieje
			for child in color_rect.get_children():
				if child is TextureRect:
					color_rect.remove_child(child)
					child.queue_free()

			# Dodaj nowy TextureRect
			var texture_rect = TextureRect.new()
			color_rect.add_child(texture_rect)
			texture_rect.expand_mode = TextureRect.EXPAND_IGNORE_SIZE  # lub EXPAND_KEEP_SIZE
			texture_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED

			# Ustaw styl rozciągania i rozmiary
			texture_rect.anchor_left = 0
			texture_rect.anchor_top = 0
			texture_rect.anchor_right = 1
			texture_rect.anchor_bottom = 1
			texture_rect.offset_left = 0
			texture_rect.offset_top = 0
			texture_rect.offset_right = 0
			texture_rect.offset_bottom = 0
			texture_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT
			texture_rect.size_flags_horizontal = Control.SIZE_EXPAND_FILL
			texture_rect.size_flags_vertical = Control.SIZE_EXPAND_FILL

			# Załaduj i ustaw teksturę
			var texture = load(textures[i])
			if texture:
				texture_rect.texture = texture
				texture_rect.visible = true
	else:
		var target_anchor_top = 1.05
		var target_anchor_bottom = 1.2

		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property(special_cards, "anchor_top", target_anchor_top, 0.5)
		tween.tween_property(special_cards, "anchor_bottom", target_anchor_bottom, 0.5)
		tween.set_parallel(false)
		tween.tween_callback(Callable(special_cards, "hide"))


func _on_laon_button_pressed() -> void:
	var player = game_manager.getCurrentPlayer() as Figurehead
	var player_index = game_manager.GetCurrentPlayerIndex()

	if !player.hasLoan:
		var loan_popup = LoanPopupScene.instantiate()
		add_child(loan_popup)
		
		loan_popup.confirm_button.pressed.connect(func():
			var amount_taken = loan_popup.selected_amount
			var topay_amount = loan_popup.topay_amount
			print("Gracz", player, "wziął pożyczkę na:", amount_taken, "ECTS")
			game_manager.AddEctsToPlayer(player_index, amount_taken)
			player.hasLoan = true
			player.Loan = topay_amount
		)
	else:
		var loan_pay_popup = LoanPayPopupScene.instantiate()
		add_child(loan_pay_popup)
		loan_pay_popup.topay_amount = player.Loan
		loan_pay_popup.amountselect.max_value = player.Loan
		loan_pay_popup.pay_amount.text = " " + str(player.Loan) + " ECTS"

		loan_pay_popup.confirm_button.pressed.connect(func():
			var amount_paid = loan_pay_popup.selected_amount
			print("Gracz", player, "spłacił:", amount_paid, "ECTS")
			
			game_manager.SubtractEctsFromPlayer(player_index, amount_paid)
			player.Loan -= amount_paid

			if player.Loan <= 0:
				player.Loan = 0
				player.hasLoan = false
				print("Gracz", player, "spłacił całą pożyczkę!")
		)


			
	
	
	
	
