extends TabContainer

@onready var achievements_tab = $Achievements

func _ready() -> void:
	self.connect("tab_changed", _on_tab_changed,0)

func _on_tab_changed(tab_index: int) -> void:
	var current_tab = self.get_child(tab_index)

	if current_tab == achievements_tab:
		print("Switched to Achievements tab")
		achievements_tab.load_achievements()
