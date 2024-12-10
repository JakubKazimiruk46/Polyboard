extends PanelContainer

var total_time_in_secs : int = 0
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	$MarginContainer/HBoxContainer/Timer.start()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func on_timer_timeout():
	total_time_in_secs+=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$MarginContainer/HBoxContainer/Time.text = '%02d:%02d' % [m, s]
