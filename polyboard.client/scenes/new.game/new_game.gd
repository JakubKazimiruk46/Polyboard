class_name NewGame
extends Control

@onready var public_checkbox =  $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/public_checkbox as Button
@onready var private_checkbox = $MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/private_checkbox as Button

func _ready():
	public_checkbox.button_pressed.connect(on_public_checkbox_pressed)
	public_checkbox.button_pressed.connect(on_private_checkbox_pressed)

func on_public_checkbox_pressed(checked):
	if checked:
		private_checkbox.set_checked(false)
		
func on_private_checkbox_pressed(checked):
	if checked:
		public_checkbox.set_checked(false)
