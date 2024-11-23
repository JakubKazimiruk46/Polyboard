extends Camera3D

# Referencja do kostki, za którą podąża kamera
@export var dice: RigidBody3D

# Prędkość ruchu kamery, by gładko podążała za kostką
@export var follow_speed: float = 5.0
# Wartość dystansu, jaki kamera powinna utrzymywać od kostki
@export var distance: float = 10.0
# Wysokość, na jakiej kamera znajduje się nad kostką
@export var height: float = 5.0

func _process(delta):
	if dice:
		# Celowa pozycja kamery
		var target_position = dice.global_transform.origin + Vector3(0, height, -distance)
		# Interpolacja, aby ruch był płynny
		global_transform.origin = global_transform.origin.lerp(target_position, follow_speed * delta)
