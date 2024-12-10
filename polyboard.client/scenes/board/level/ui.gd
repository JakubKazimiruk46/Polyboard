extends CanvasLayer

var cards_view = false
var buttons_view = false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass
func on_cards_button_pressed():
	if cards_view == false:
		cards_view = true
		var target_anchor_top = 0.85  # np. nowa wartość proporcjonalna
		var target_achor_bottom = 1
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Cards,"anchor_top",target_anchor_top,0.5)
		tween.tween_property($Cards,"anchor_bottom",target_achor_bottom,0.5)
		tween.set_parallel(false)
		
	elif cards_view == true:
		var target_anchor_top = 1.05  # np. nowa wartość proporcjonalna
		var target_anchor_bottom = 1.2
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($Cards,"anchor_top",target_anchor_top,0.5)
		tween.tween_property($Cards,"anchor_bottom",target_anchor_bottom,0.5)
		tween.set_parallel(false)
		cards_view = false
# Called every frame. 'delta' is the elapsed time since the previous frame.
func on_view_buttons_pressed():
	if buttons_view == false:
		buttons_view = true
		var target_anchor_left = 1
		var target_anchor_right = 1
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($HBoxContainer,"anchor_left",target_anchor_left,0.5)
		tween.tween_property($HBoxContainer,"anchor_right",target_anchor_right,0.5)
		tween.set_parallel(false)
	elif buttons_view == true:
		var target_anchor_left = 0.95  # np. nowa wartość proporcjonalna
		var target_anchor_right = 1.14
		var tween = create_tween()
		tween.set_parallel(true)
		tween.tween_property($HBoxContainer,"anchor_left",target_anchor_left,0.5)
		tween.tween_property($HBoxContainer,"anchor_right",target_anchor_right,0.5)
		tween.set_parallel(false)
		buttons_view = false
		
func _process(delta: float) -> void:
		pass
