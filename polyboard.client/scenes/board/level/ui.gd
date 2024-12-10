extends CanvasLayer

var cards_view = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass
func on_button_pressed():
	if cards_view == false:
		cards_view = true
		var target_anchor_top = 0.7  # np. nowa wartość proporcjonalna
		var target_achor_bottom = 1
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Buttons,"anchor_top",target_anchor_top,0.5)
		tween.tween_property($Buttons,"anchor_bottom",target_achor_bottom,0.5)
		tween.set_parallel(false)
		
	elif cards_view == true:
		var target_anchor_top = 0.95  # np. nowa wartość proporcjonalna
		var target_anchor_bottom = 1.25
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Buttons,"anchor_top",target_anchor_top,0.5)
		tween.tween_property($Buttons,"anchor_bottom",target_anchor_bottom,0.5)
		tween.set_parallel(false)
		cards_view = false
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
