extends Node

class_name PasswordValidator

#Walidacja haseł
#Co najmniej jedna mała, jedna duża, jeden specjalny, dwa unikanlne i jedna cyfra
func validate_password(password: String) -> String:
	var password_regex = RegEx.new()
	var regex_pattern = "^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\":{}|<>])[A-Za-z0-9!@#$%^&*(),.?\":{}|<>]{8,}$"

	if password.length() < 8:
		return "Password must be at least 8 characters long!"
	
	if not password_regex.compile(regex_pattern):
		return "Password must contain at least one upeercase, one lowercase letter, one number and one special character!"

	var unique_chars = {}
	for c in password:
		unique_chars[c] = true
	if unique_chars.size() < 2:
		return "Password must contain at least two unique characters."

	return "valid"
