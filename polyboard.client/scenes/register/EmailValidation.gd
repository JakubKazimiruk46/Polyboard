extends Node

class_name EmailValidator

func validate_email(email: String) -> String:
	var regex_pattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$"
	var regex = RegEx.new()
	
	var compile_result = regex.compile(regex_pattern)
	if compile_result != OK:
		print("Regex compilation failed")
		return "Invalid regex pattern"
	
	var match = regex.search(email)
	if match:
		print("Match found: ", match.get_string())
		return "valid"
	else:
		print("No match found.")
		return "Email must contain exactly one '@' symbol and end with a proper domain (e.g., '.com')."
	return "valid"
