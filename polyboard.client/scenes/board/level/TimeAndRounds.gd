extends PanelContainer

var total_time_in_secs : int = 0
var paused := false
var turn_timer = null

@onready var game_timer = $HBoxContainer/MarginContainer/HBoxContainer/Timer
@onready var time_label = $HBoxContainer/MarginContainer/HBoxContainer/Time

func _ready() -> void:
	game_timer.start()

	var panel_style = StyleBoxFlat.new()
	panel_style.bg_color = Color(0.2, 0.2, 0.2, 0.95)
	panel_style.border_width_bottom = 3
	panel_style.border_width_left = 3
	panel_style.border_width_right = 3
	panel_style.border_width_top = 3
	panel_style.border_color = Color("#62ff45")
	panel_style.corner_radius_bottom_left = 8
	panel_style.corner_radius_bottom_right = 8
	panel_style.corner_radius_top_left = 8
	panel_style.corner_radius_top_right = 8

	add_theme_stylebox_override("panel", panel_style)

func on_timer_timeout():
	if paused:
		return
	total_time_in_secs += 1
	var m = int(total_time_in_secs / 60.0)
	var s = total_time_in_secs - m * 60
	time_label.text = '%02d:%02d' % [m, s]

func _on_pause_button_pressed() -> void:
	paused = !paused
	game_timer.set_paused(paused)

	# spróbuj znaleźć TurnTimer dynamicznie dopiero przy kliknięciu
	if turn_timer == null:
		var gm = get_node_or_null("/root/Level/GameManager")
		if gm:
			for child in gm.get_children():
				if child is Timer and child.one_shot:
					turn_timer = child
					print("🔍 TurnTimer znaleziony dynamicznie:", turn_timer)

	if turn_timer:
		turn_timer.set_paused(paused)
	else:
		print("⚠ TurnTimer nadal nie znaleziony – może nie został jeszcze dodany.")
