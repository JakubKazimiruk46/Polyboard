extends TextureRect

@onready var closebutton = $close
@onready var amountselect = $SpinBox
@onready var confirm_button = $confirm
@onready var pay_amount = $pay_amount

var selected_amount: int = 0
var topay_amount:int= 0

func _ready():
	confirm_button.pressed.connect(_on_confirm_button_pressed)
	closebutton.pressed.connect(_on_close_button_pressed)
	pay_amount.text = " " + str(topay_amount) + " ECTS"



func _on_confirm_button_pressed():
	selected_amount=amountselect.value
	hide()  

func _on_close_button_pressed():
	hide()  
