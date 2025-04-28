extends PanelContainer

var total_time_in_secs : int = 0

func _ready() -> void:
	$HBoxContainer/MarginContainer/HBoxContainer/Timer.start()
	
	# Apply custom style to match popup menu and notification
	var panel_style = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.2, 0.2, 0.2, 0.95) # Dark background
	panel_style.border_width_bottom = 3
	panel_style.border_width_left = 3
	panel_style.border_width_right = 3
	panel_style.border_width_top = 3
	panel_style.border_color = Color("#62ff45") # Green border like notification
	panel_style.corner_radius_bottom_left = 8
	panel_style.corner_radius_bottom_right = 8
	panel_style.corner_radius_top_left = 8
	panel_style.corner_radius_top_right = 8
	
	# Apply the style to this PanelContainer
	add_theme_stylebox_override("panel", panel_style)

func on_timer_timeout():
	total_time_in_secs+=1
	var m = int(total_time_in_secs/60.0)
	var s = total_time_in_secs - m * 60
	$HBoxContainer/MarginContainer/HBoxContainer/Time.text = '%02d:%02d' % [m, s]
