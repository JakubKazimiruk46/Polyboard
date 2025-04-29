extends TextureRect

@onready var closebutton = $close
@onready var amountselect = $SpinBox
@onready var pay_amount = $pay_amount
@onready var confirm_button = $confirm

var selected_amount: int = 0
var topay_amount:int =0

func _ready():
	amountselect.value_changed.connect(_on_amountselect_value_changed)
	confirm_button.pressed.connect(_on_confirm_button_pressed)
	closebutton.pressed.connect(_on_close_button_pressed)
	_update_pay_amount()

func _on_amountselect_value_changed(value):
	_update_pay_amount()

func _update_pay_amount():
	var base_amount = amountselect.value
	var final_amount = round(base_amount * 1.4)
	pay_amount.text = " " + str((final_amount)) + " ECTS"

func _on_confirm_button_pressed():
	topay_amount = round(amountselect.value * 1.4)
	selected_amount=amountselect.value
	hide()  

func _on_close_button_pressed():
	hide()  
