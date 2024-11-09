extends Node

class_name EmailValidator

func validate_email(email: String) -> String:
	var  monkey_regex = RegEx.new()
	monkey_regex.compile("@")
	
	var domain_regex = RegEx.new()
	domain_regex.compile("(?<=@)[A-Za-z0-9.-]+")
	
	var end_regex = RegEx.new()
	end_regex.compile("\\.[A-Za-z]{2,}$")
	
	var username_regex = RegEx.new()
	username_regex.compile("^[A-Za-z0-9._%+-]+")
	
	if not username_regex.search(email):
		return "Email must contain valid username."
	
	if not monkey_regex.search(email):
		return "Email must contain @ symbol."
	
	if not domain_regex.search(email):
		return "Email must contain domain name."
	
	if not end_regex.search(email):
		return "Email must end with valid domain ('.com', '.pl'. '.org' etc.)."
	
	
	return "valid"
