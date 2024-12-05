extends RigidBody3D


@onready var raycasts = $Raycasts.get_children()
@onready var dice_sound_player = $AudioStreamPlayer



var start_pos
var roll_strength = 25

var is_rolling = false


signal roll_finished(value)


func _ready():
	start_pos = global_position


func _input(event):
	if event.is_action_pressed("ui_accept") && !is_rolling:
		_roll()


func _roll():
	# Reset state
	sleeping = false
	freeze = false
	transform.origin = start_pos
	linear_velocity = Vector3.ZERO
	angular_velocity = Vector3.ZERO

	# Randomowa rotacja
	transform.basis = Basis(Vector3.RIGHT, randf_range(0, 2 * PI)) * transform.basis
	transform.basis = Basis(Vector3.UP, randf_range(0, 2 * PI)) * transform.basis
	transform.basis = Basis(Vector3.FORWARD, randf_range(0, 2 * PI)) * transform.basis

	# Losowy rzut
	var throw_vector = Vector3(randf_range(-1, 1), 0, randf_range(-1, 1)).normalized()
	angular_velocity = throw_vector * roll_strength / 2
	apply_central_impulse(throw_vector * roll_strength)
	is_rolling = true

	# Odtwórz dźwięk rzutu
	play_dice_sound_with_delay(0.5) 




func _on_sleeping_state_changed() -> void:
	if sleeping:
		var landed_on_side = false
		for raycast in raycasts:
			if raycast.is_colliding():
				roll_finished.emit(raycast.opposite_side)
				is_rolling = false
				landed_on_side = true
					
					
		if !landed_on_side:
					_roll()
					
# Odtwórz dźwięk rzutu z opóźnieniem
func play_dice_sound_with_delay(delay_time: float) -> void:
	if dice_sound_player != null:
		await get_tree().create_timer(delay_time).timeout
		dice_sound_player.play()
	else:
		print("AudioStreamPlayer nie jest przypisany!")
