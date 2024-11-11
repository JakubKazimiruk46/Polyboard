extends Node

class_name PasswordValidator

#Walidacja haseł
#Co najmniej jedna mała, jedna duża, jeden specjalny, dwa unikanlne
#i jedna cyfra 

func validate_password(password: String) -> String:

	if password.length() < 8:
		return "Password must be at least 8 characters long!"

	var unique_chars = {}
	for c in password:
		unique_chars[c] = true
	if unique_chars.size() < 2:
		return "Password must contain at least two unique characters."

	var lowercase_regex = RegEx.new()
	lowercase_regex.compile("(?=.*[a-z])")
	
	var uppercase_regex = RegEx.new()
	uppercase_regex.compile("(?=.*[A-Z])")
	
	var digit_regex = RegEx.new()
	digit_regex.compile("(?=.*\\d)")
	
	var special_char_regex = RegEx.new()
	special_char_regex.compile("(?=.*[@$!%*?&#])")

	if not lowercase_regex.search(password):
		return "Password must contain at least one lowercase letter."
	
	if not uppercase_regex.search(password):
		return "Password must contain at least one uppercase letter."
	
	if not digit_regex.search(password):
		return "Password must contain at least one digit."
	
	if not special_char_regex.search(password):
		return "Password must contain at least one special character (@, $, !, %, *, ?, &)."

	return "valid"
