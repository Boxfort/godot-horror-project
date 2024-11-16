shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform int color_depth = 5;

void fragment() {
	// Convert from 0 to 1 to 0 to 255.
	ivec3 c = ivec3(round(COLOR.rgb * 255.0));
	
	// Truncate from 8 bits to color_depth bits.
	c >>= (8 - color_depth);

	// Convert back to 0 to 1.
	COLOR.rgb = vec3(c) / float(1 << color_depth);

	float avg = (COLOR.r + COLOR.g + COLOR.b) / 3.0;
	COLOR.rgb = vec3(avg);
}
