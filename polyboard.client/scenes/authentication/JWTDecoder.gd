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
	print("Payload_json: ", payload_json)
	var parse_result = json.parse(payload_json)

	if parse_result != OK:
		print("Failed to decode JWT payload")
		return {}
		
	var json_data = json.data

	return json_data as Dictionary

func decode_base64(base64_string: String) -> String:
	var repaired_base64 = base64_string.replace("-", "+").replace("_", "/")
	while repaired_base64.length() % 4 != 0:
		repaired_base64 += "="
	
	var decoded_bytes = Marshalls.base64_to_raw(repaired_base64)
	return decoded_bytes.get_string_from_utf8()
