extends CanvasLayer

@onready var notification_label = $NotificationPanel/NotificationLabel
@onready var notification_panel = $NotificationPanel

var original_position_x: float = 0
var fade_tween: Tween

const COLOR_NORMAL := Color("#62ff45")
const COLOR_ERROR := Color("#ff5757")
const FADE_DURATION := 2

func _ready():
	notification_panel.visible = false
	notification_label.visible = false
	original_position_x = notification_panel.position.x
	notification_label.add_theme_font_size_override("font_size", 19)

func show_notification(message: String, duration: float = 3.0) -> void:
	_display_notification(message, COLOR_NORMAL, duration)

func show_error(message: String, duration: float = 4.0) -> void:
	_display_notification(message, COLOR_ERROR, duration)
	push_error(message)

func _display_notification(message: String, color: Color, duration: float) -> void:
	if fade_tween and fade_tween.is_valid():
		fade_tween.kill()
		
	notification_label.text = message
	notification_label.add_theme_color_override("font_color", color)
	
	notification_panel.visible = true
	notification_label.visible = true

	notification_panel.position.x = original_position_x
	notification_panel.modulate.a = 1
	notification_label.modulate.a = 1
	
	var shake_tween = create_tween()
	shake_tween.tween_property(notification_panel, "position:x", original_position_x - 10, 0.05)
	shake_tween.tween_property(notification_panel, "position:x", original_position_x + 10, 0.05)
	shake_tween.tween_property(notification_panel, "position:x", original_position_x, 0.05)
	
	var timer = get_tree().create_timer(duration)
	timer.timeout.connect(_start_fade_out)

func _start_fade_out() -> void:
	fade_tween = create_tween()
	
	fade_tween.set_trans(Tween.TRANS_SINE)
	fade_tween.set_ease(Tween.EASE_IN_OUT)
	
	notification_panel.modulate.a = 1.0
	notification_label.modulate.a = 1.0

	fade_tween.tween_property(notification_panel, "modulate:a", 0.0, FADE_DURATION)
	fade_tween.parallel().tween_property(notification_label, "modulate:a", 0.0, FADE_DURATION)

	fade_tween.finished.connect(_hide_notification)
	
	print("Starting fade out animation")

func _hide_notification() -> void:
	notification_panel.visible = false
	notification_label.visible = false
	notification_label.remove_theme_color_override("font_color")
