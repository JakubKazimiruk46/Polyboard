extends Node

class_name JwtDecoder

func decode_jwt(token: String) -> Dictionary:
	var parts = token.split(".")
	
	if parts.size() != 3:
		print("Invalid JWT token")
		return {}

	var payload_base64 = parts[1]
	var payload_json = decode_base64(payload_base64)
	
	var json = JSON.new()
	var parse_result = json.parse(payload_json)

	if parse_result != OK:
		print("Failed to decode JWT payload")
		return {}
		
	print("JSON DATA: ", json.result)

	return json.result

func decode_base64(base64_string: String) -> String:
	var decoded_bytes = base64_string.base64_to_raw() 
	
	return decoded_bytes.get_string_from_utf8()
