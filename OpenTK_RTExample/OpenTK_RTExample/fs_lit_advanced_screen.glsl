#version 330

// Holds information about a light
struct Light {
	vec3 position;
	vec3 color;
	float ambientIntensity;
	float diffuseIntensity;

	int type;
	vec3 direction;
	float coneAngle;

	float attenuationConstant;
	float attenuationLinear;
	float attenuationQuadratic;
	float radius;
};

in vec3 v_norm;
in vec3 v_pos;
in vec2 f_texcoord;
out vec4 outputColor;

// Texture information
uniform sampler2D maintexture;
uniform bool hasSpecularMap;
uniform sampler2D map_specular;

uniform mat4 view;

// Material information
uniform vec3 material_ambient;
uniform vec3 material_diffuse;
uniform vec3 material_specular;
uniform float material_specExponent;

// Array of lights used in the shader
uniform Light lights[5];

uniform float time;

// Thanks to http://byteblacksmith.com/improvements-to-the-canonical-one-liner-glsl-rand-for-opengl-es-2-0/
highp float rand(vec2 co)
{
    highp float a = 12.9898;
    highp float b = 78.233;
    highp float c = 43758.5453;
    highp float dt= dot(co.xy ,vec2(a,b));
    highp float sn= mod(dt,3.14);
    return fract(sin(sn) * c);
}

void
main()
{
	// Texture information
	vec2 flipped_texcoord = vec2(1.0 - f_texcoord.x, 1.0 - f_texcoord.y);
	vec4 texcolor = texture2D(maintexture, flipped_texcoord.xy);

	// Add some noise to make it look cool
	float noiseVal = rand(flipped_texcoord + vec2(mod(time, 2.0), mod(time, 10.0)));
	vec4 noise = vec4(noiseVal,noiseVal,noiseVal,0);
	texcolor = 0.8 * texcolor + 0.2f * noise;
	

	vec3 n = normalize(v_norm);
	
	// Loop through lights, adding the lighting from each one
	for(int i = 0; i < 5; i++){
		
		// Skip lights with no effect
		if(lights[i].color == vec3(0,0,0))
		{
			continue;
		}
		
		vec3 lightvec = normalize(lights[i].position - v_pos);
		vec4 lightcolor = vec4(0,0,0,1);

		// Check spotlight angle
		bool inCone = false;
		if(lights[i].type == 1 && degrees(acos(dot(lightvec, lights[i].direction))) < lights[i].coneAngle)
		{
			inCone = true;
		}

		// Directional lighting
		if(lights[i].type == 2){
			lightvec = lights[i].direction;
		}

		// Colors
		vec4 light_ambient = lights[i].ambientIntensity * vec4(lights[i].color, 0.0);
		vec4 light_diffuse = lights[i].diffuseIntensity * vec4(lights[i].color, 0.0);

		// Ambient lighting
		lightcolor = lightcolor + texcolor * light_ambient * vec4(material_ambient, 0.0);

		// Diffuse lighting
		float lambertmaterial_diffuse = max(dot(n, lightvec), 0.0);

		// Spotlight, limit light to specific angle
		if(lights[i].type != 1 || inCone){
			lightcolor = lightcolor + (light_diffuse * texcolor * vec4(material_diffuse, 0.0)) * lambertmaterial_diffuse;
		}

		// Specular lighting
		vec3 reflectionvec = normalize(reflect(-lightvec, v_norm));
		vec3 viewvec = normalize(vec3(inverse(view) * vec4(0,0,0,1)) - v_pos); 
		float material_specularreflection = max(dot(v_norm, lightvec), 0.0) * pow(max(dot(reflectionvec, viewvec), 0.0), material_specExponent);

		// Specular map
		if(hasSpecularMap)
		{
			material_specularreflection = material_specularreflection *	texture2D(map_specular, flipped_texcoord.xy).r;
		}

		// Spotlight, specular reflections are also limited by angle
		if(lights[i].type != 1 || inCone){
			lightcolor = lightcolor + vec4(material_specular * lights[i].color, 0.0) * material_specularreflection;
		}

		// Attenuation
		float distancefactor = distance(lights[i].position, v_pos);
		float attenuation = 1.0 / (1.0 + (distancefactor * lights[i].attenuationLinear) + (distancefactor * distancefactor * lights[i].attenuationQuadratic));
		outputColor = outputColor + lightcolor * attenuation;
	}

}