extends CanvasLayer

@onready var notification_label = $NotificationPanel/NotificationLabel
@onready var notification_panel = $NotificationPanel

var fade_tween: Tween
var slide_tween: Tween

const COLOR_NORMAL := Color("#62ff45")
const COLOR_ERROR := Color("#ff0015")
const FADE_DURATION := 1.2
const SLIDE_DURATION := 0.4

func _ready():
	notification_panel.visible = false
	notification_label.visible = false
	
	
	notification_label.add_theme_font_size_override("font_size", 19)

func show_notification(message: String, duration: float = 3.0) -> void:
	_display_notification(message, COLOR_NORMAL, duration)

func show_error(message: String, duration: float = 4.0) -> void:
	_display_notification(message, COLOR_ERROR, duration)
	push_error(message)

func _display_notification(message: String, color: Color, duration: float) -> void:
	if fade_tween and fade_tween.is_valid():
		fade_tween.kill()
	if slide_tween and slide_tween.is_valid():
		slide_tween.kill()
		
	notification_label.text = message
	notification_label.add_theme_color_override("font_color", color)

	notification_panel.modulate.a = 1
	notification_label.modulate.a = 1
	
	notification_panel.visible = true
	notification_label.visible = true
	
	slide_tween = create_tween()
	slide_tween.set_trans(Tween.TRANS_BACK)
	slide_tween.set_ease(Tween.EASE_OUT)
	slide_tween.tween_property(notification_panel, "position:y", 0, SLIDE_DURATION)
	slide_tween.finished.connect(_start_shake_effect)
	
	var timer = get_tree().create_timer(duration)
	timer.timeout.connect(_start_fade_out)

func _start_shake_effect() -> void:
	var shake_tween = create_tween()
	var current_x = notification_panel.position.x
	shake_tween.tween_property(notification_panel, "position:x", current_x - 10, 0.05)
	shake_tween.tween_property(notification_panel, "position:x", current_x + 10, 0.05)
	shake_tween.tween_property(notification_panel, "position:x", current_x, 0.05)

func _start_fade_out() -> void:
	fade_tween = create_tween()
	
	fade_tween.set_trans(Tween.TRANS_SINE)
	fade_tween.set_ease(Tween.EASE_IN_OUT)
	
	fade_tween.tween_property(notification_panel, "modulate:a", 0.0, FADE_DURATION)
	fade_tween.parallel().tween_property(notification_label, "modulate:a", 0.0, FADE_DURATION)

	fade_tween.finished.connect(_hide_notification)
	
	print("Starting fade out animation")

func _hide_notification() -> void:
	notification_panel.visible = false
	notification_label.visible = false
	notification_label.remove_theme_color_override("font_color")
