#version 330

in vec3 v_norm;
in vec3 v_pos;
in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D maintexture;
uniform mat4 view;

uniform vec3 material_ambient;
uniform vec3 material_diffuse;
uniform vec3 material_specular;
uniform float material_specExponent;

uniform vec3 light_position;
uniform vec3 light_color;
uniform float light_ambientIntensity;
uniform float light_diffuseIntensity;

void
main()
{
	vec2 flipped_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
	vec3 n = normalize(v_norm);

	// Colors
	vec4 texcolor = texture2D(maintexture, flipped_texcoord.xy);
	vec4 light_ambient = light_ambientIntensity * vec4(light_color, 0.0);
	vec4 light_diffuse = light_diffuseIntensity * vec4(light_color, 0.0);

	// Ambient lighting
	outputColor = texcolor * light_ambient * vec4(material_ambient, 0.0);

	// Diffuse lighting
	vec3 lightvec = normalize(light_position - v_pos);
	float lambertmaterial_diffuse = max(dot(n, lightvec), 0.0);
	outputColor = outputColor + (light_diffuse * texcolor * vec4(material_diffuse, 0.0)) * lambertmaterial_diffuse;

	// Specular lighting
	vec3 reflectionvec = normalize(reflect(-lightvec, v_norm));
	vec3 viewvec = normalize(vec3(inverse(view) * vec4(0,0,0,1)) - v_pos); 
	float material_specularreflection = max(dot(v_norm, lightvec), 0.0) * pow(max(dot(reflectionvec, viewvec), 0.0), material_specExponent);
	outputColor = outputColor + vec4(material_specular * light_color, 0.0) * material_specularreflection;
}