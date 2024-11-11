extends Control

@onready var option_button = $HBoxContainer/OptionButton as OptionButton

const FILTER_MODE_ARRAY : Array[String] = [
	"All",
	"Achieved",
	"Not achieved"
]

func _ready():
	add_filter_mode_items()
	option_button.item_selected.connect(on_filter_mode_selected)

func add_filter_mode_items() -> void:
	for window_mode in FILTER_MODE_ARRAY:
		option_button.add_item(window_mode)

func on_filter_mode_selected(index: int) -> void:
	pass
