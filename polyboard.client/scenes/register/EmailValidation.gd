extends Node

class_name EmailValidator

func validate_email(email: String) -> String:
	
	var regex_pattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{3}$"
	var regex = RegEx.new()
	regex.compile(regex_pattern)
	
	if not regex.search(email):
		return "Email must contain exactly one '@' character and end with domain"
	
	var at_count = email.count("@")
	if at_count != 1:
		return "Email must contain exactly one '@' symbol."

	return "valid"
