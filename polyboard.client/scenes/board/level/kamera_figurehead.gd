extends Camera3D

@export var target: Node3D  # Obiekt, za którym kamera ma podążać
@export var distance_behind: float = 5.0   # Dystans kamery za obiektem
@export var height_above: float = 2.0      # Wysokość kamery względem obiektu
@export var smooth_speed: float = 0.1      # Płynność ruchu kamery

# Przechowuje ostatnią pozycję, aby obliczyć kierunek ruchu
var last_position: Vector3 = Vector3()

func _ready():
	if target:
		last_position = target.global_transform.origin

func _process(_delta):
	if not target:
		return

	# Oblicz kierunek ruchu na podstawie zmiany pozycji
	var movement_direction = (target.global_transform.origin - last_position).normalized()
	
	# Sprawdź, czy figurehead się porusza
	if movement_direction.length() > 0.01:
		# Ustaw pozycję kamery za obiektem w kierunku ruchu
		var target_position = target.global_transform.origin - movement_direction * distance_behind + Vector3(0, height_above, 0)
		global_position = lerp(global_position, target_position, smooth_speed)
		
		# Kamera patrzy na pozycję figurehead
		look_at(target.global_transform.origin, Vector3.UP)
	
	# Zaktualizuj ostatnią pozycję
	last_position = target.global_transform.origin
