shader_type spatial;
render_mode unshaded, diffuse_lambert, cull_disabled, shadows_disabled, depth_draw_opaque;

uniform float precision_multiplier : hint_range(0.0, 1.0) = 0.75;
uniform vec4 modulate_color : source_color = vec4(1.0); 
uniform sampler2D albedoTex: source_color, filter_nearest;
uniform vec2 uv_scale = vec2(4.3, 1.625);
uniform vec2 uv_offset = vec2(2.85, -0.241);
uniform vec2 uv_pan_velocity = vec2(0.0);

uniform vec2 uv_pivot = vec2(0, 0);
uniform float uv_rotation = 1.571;
uniform bool affine_mapping = false;

vec2 rotateUV(vec2 uv, vec2 pivot, float rotation) {
    float cosa = cos(rotation);
    float sina = sin(rotation);
    uv -= pivot;
    return vec2(
        cosa * uv.x - sina * uv.y,
        cosa * uv.y + sina * uv.x 
    ) + pivot;
}

// https://github.com/dsoft20/psx_retroshader/blob/master/Assets/Shaders/psx-vertexlit.shader
const vec2 base_snap_res = vec2(160.0, 120.0);
vec4 get_snapped_pos(vec4 base_pos)
{
	vec4 snapped_pos = base_pos;
	snapped_pos.xyz = base_pos.xyz / base_pos.w; // convert to normalised device coordinates (NDC)
	vec2 snap_res = floor(base_snap_res * precision_multiplier);  // increase "snappy-ness"
	snapped_pos.x = floor(snap_res.x * snapped_pos.x) / snap_res.x;  // snap the base_pos to the lower-vertex_resolution grid
	snapped_pos.y = floor(snap_res.y * snapped_pos.y) / snap_res.y;
	snapped_pos.xyz *= base_pos.w;  // convert back to projection-space
	return snapped_pos;
}

void vertex()
{
	UV = UV * uv_scale + uv_offset;
	UV += uv_pan_velocity * TIME;
	
	UV = rotateUV(UV, uv_pivot, uv_rotation);

	POSITION = get_snapped_pos(PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0));  // snap position to grid
	
    if (affine_mapping) {
	   POSITION /= abs(POSITION.w);  // discard depth for affine mapping
    }

	VERTEX = VERTEX;  // it breaks without this - not sure why
}

uniform float saturation = 1;
uniform float brightness = 1;
uniform float contrast = 1;

void fragment()
{
	//vec4 tex = texture(albedoTex, UV) * modulate_color;
	//tex.rgb = mix(vec3(0.0), tex.rgb, brightness);
    //tex.rgb = mix(vec3(0.5), tex.rgb, contrast);
    //tex.rgb = mix(vec3(dot(vec3(1.0), tex.rgb) * 0.33333), tex.rgb, saturation);
	//ALBEDO = tex.rgb * COLOR.rgb;
	vec4 tex = (COLOR * texture(albedoTex, UV) * modulate_color);
	ALBEDO = tex.rgb;
	ALPHA = tex.a;
}
