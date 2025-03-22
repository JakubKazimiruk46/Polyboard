extends CanvasLayer

@onready var endturnbutton = $"../UI/ZakończTure"
@onready var tradeButton = $"../UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button" as TextureButton
@onready var buildButton = $"../UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button" as TextureButton

var board_view = false
var total_time_in_secs : int = 30
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func _on_viewBoardButtonPressed():
	if(board_view == false):
		$HBoxContainer/FieldView.visible=false;
		$HBoxContainer/VBoxContainer/BuyPanel.visible=false
		$Blur.visible = false
		$HBoxContainer/VBoxContainer/ViewBoardButton.text="VIEW BUY OFFER"
		board_view = true
	else:
		$HBoxContainer/FieldView.visible=true;
		$HBoxContainer/VBoxContainer/BuyPanel.visible=true
		$Blur.visible=true
		$HBoxContainer/VBoxContainer/ViewBoardButton.text="VIEW BOARD"
		board_view = false
	
func on_timer_timeout():
	total_time_in_secs-=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = '%02d:%02d' % [m, s]
	if total_time_in_secs == 5:
		$Ticking.play()
		$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.add_theme_color_override("font_color","red")
	if total_time_in_secs == 0:
		$Timer.stop()
		$Ticking.stop()
		self.visible = false
		total_time_in_secs = 30
		$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.add_theme_color_override("font_color","white")
		$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''
		turn_on_buttons()

func turn_on_buttons():
	endturnbutton.visible = true
	tradeButton.disabled = false
	buildButton.disabled = false

func on_buyButtonPressed():
	var figurehead_script = preload("res://scenes/board/figurehead/Figurehead.cs")
	var board = $"../Board"
	var game_manager = $"../GameManager"
	var currentFigureHead = game_manager.getCurrentPlayer()
	var current_position = currentFigureHead.GetCurrentPositionIndex()
	var Field = game_manager.getCurrentField(current_position)
	if board and board.has_method("GetFieldById"):
		# Pobierz pole jako Node (lub Field, jeśli typowanie jest zaimplementowane)
		print(Field.FieldId)
		print(current_position)
		if Field && Field.fieldCost <= currentFigureHead.ECTS:
			Field.BuyField(currentFigureHead,Field)
			$Timer.stop()
			$ZakupKarty.play()
			$HBoxContainer/GPUParticles2D.emitting = true
			await get_tree().create_timer(1.5).timeout
			$HBoxContainer/GPUParticles2D.emitting = false
			await get_tree().create_timer(1).timeout
			self.visible=false
			$Ticking.stop()
			total_time_in_secs = 30
			$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''
			$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.add_theme_color_override("font_color","white")
			turn_on_buttons()
		#else:
		#	var n = "Nie masz wystarczającej ilości ECTS, aby kupić to pole"
		#	game_manager.ShowNotification(n)
			#print("Nie znaleziono instancji Board lub metoda GetFieldById nie istnieje!")

func on_auctionButtonPressed():
	self.visible = false
	$Timer.stop()
	$Ticking.stop()
	total_time_in_secs = 30
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.add_theme_color_override("font_color","white")
	turn_on_buttons()
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
