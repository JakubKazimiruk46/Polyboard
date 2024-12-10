shader_type canvas_item;

uniform float rotation_angle : hint_range(0.0, 360.0);

void fragment() {
    // Simple rotation logic for debugging
    vec2 center = vec2(0.5, 0.5);
    float rad = radians(rotation_angle);

    vec2 uv_rotated;
    uv_rotated.x = cos(rad) * (FRAGCOORD.x - center.x) - sin(rad) * (FRAGCOORD.y - center.y) + center.x;
    uv_rotated.y = sin(rad) * (FRAGCOORD.x - center.x) + cos(rad) * (FRAGCOORD.y - center.y) + center.y;

    // Ensure the uv is inside the valid range [0,1]
    uv_rotated = clamp(uv_rotated, 0.0, 1.0);

    // Sample the texture
    COLOR = texture(TEXTURE, uv_rotated);
}
