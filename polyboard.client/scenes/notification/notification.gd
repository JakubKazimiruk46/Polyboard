extends CanvasLayer

@onready var notification_label = $NotificationLabel

func show_notification(message: String, duration: float = 3.0) -> void:
	notification_label.text = message
	notification_label.visible = true
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = duration
	timer.connect("timeout", Callable(self, "_hide_notification"))
	add_child(timer)
	timer.start()

func _hide_notification() -> void:
	notification_label.visible = false
