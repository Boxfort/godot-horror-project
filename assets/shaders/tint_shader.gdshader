shader_type canvas_item;

uniform bool flip ;
uniform sampler2D gradient; // It can be whatever palette you want

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment(){ 
	vec4 col = texture(screen_texture,SCREEN_UV);
	float lum = dot(col.rgb,vec3(0.2126,0.7152,0.0722)); // luminance
	
	col = texture(gradient,vec2(abs(float(flip) - lum),0));
	
	COLOR = col;
}