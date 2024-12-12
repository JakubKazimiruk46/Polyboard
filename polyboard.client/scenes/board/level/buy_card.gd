extends CanvasLayer
var board_view = false
var total_time_in_secs : int = 30
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

func _on_viewBoardButtonPressed():
	if(board_view == false):
		$HBoxContainer/FieldView.visible=false;
		$HBoxContainer/VBoxContainer/BuyPanel.visible=false
		$HBoxContainer/VBoxContainer/ViewBoardButton.text="VIEW BUY OFFER"
		board_view = true
	else:
		$HBoxContainer/FieldView.visible=true;
		$HBoxContainer/VBoxContainer/BuyPanel.visible=true
		$HBoxContainer/VBoxContainer/ViewBoardButton.text="VIEW BOARD"
		board_view = false
	
func on_timer_timeout():
	total_time_in_secs-=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = '%02d:%02d' % [m, s]
	if total_time_in_secs == 0:
		$Timer.stop()
		self.visible = false
		total_time_in_secs = 30
		$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''

func on_buyButtonPressed():
	self.visible = false
	$Timer.stop()
	total_time_in_secs = 30
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''

func on_auctionButtonPressed():
	self.visible = false
	$Timer.stop()
	total_time_in_secs = 30
	$HBoxContainer/VBoxContainer/BuyPanel/VBoxContainer/TimeLeft.text = ''
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
