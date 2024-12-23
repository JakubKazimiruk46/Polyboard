shader_type canvas_item;
uniform float blur_size : hint_range(0.0, 20.0) = 5.0;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    vec4 color = vec4(0.0);
    vec2 size = vec2(blur_size) / SCREEN_PIXEL_SIZE;

    for (float x = -1.0; x <= 1.0; x += 1.0) {
        for (float y = -1.0; y <= 1.0; y += 1.0) {
            color += texture(TEXTURE, UV + vec2(x, y) * size);
        }
    }

    color /= 10.0; // Średnia wartość
    COLOR = color;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
