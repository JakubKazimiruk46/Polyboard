extends CanvasLayer

@onready var notification_label = $NotificationLabel
@onready var notification_panel = $NotificationPanel

func show_notification(message: String, duration: float = 3.0) -> void:
	notification_label.text = message
	notification_panel.visible = true
	notification_label.visible = true
	# Ustaw pełną widoczność
	notification_panel.modulate.a = 1
	notification_label.modulate.a = 1
	# Tworzymy tween do efektu "wstrząsu"
	var tween = create_tween()
	tween.tween_property(notification_panel, "rect_position:x", notification_panel.rect_position.x - 10, 0.05)
	tween.tween_property(notification_panel, "rect_position:x", notification_panel.rect_position.x + 10, 0.05)
	tween.tween_property(notification_panel, "rect_position:x", notification_panel.rect_position.x, 0.05)
	# Dodajemy timer do rozpoczęcia efektu zanikania
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = duration
	timer.connect("timeout", Callable(self, "_start_fade_out"))
	add_child(timer)
	timer.start()


func _start_fade_out() -> void:
	var tween = create_tween()
	tween.tween_property(notification_panel, "modulate:a", 0, 0.2) # Panel zanika
	tween.tween_property(notification_label, "modulate:a", 0, 0.2) # Label zanika
	tween.connect("finished", Callable(self, "_hide_notification"))

func _hide_notification() -> void:
	notification_panel.visible = false
	notification_label.visible = false
