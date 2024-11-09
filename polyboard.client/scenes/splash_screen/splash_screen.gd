class_name SplashScreen extends Node

@export var _time: float = 3
@export var _fade_time: float = 1

signal finished()

func start() -> void:
	self.visible=true
	var tween: Tween = create_tween()
	tween.connect("finished",_finish)
	tween.tween_property(self, "modulate",Color(1,1,1,1),_fade_time)
	tween.tween_interval(_time)
	tween.tween_property(self,"modulate",Color(0,0,0,1),_fade_time)
	
func _finish() -> void:
	emit_signal("finished")
	queue_free()
