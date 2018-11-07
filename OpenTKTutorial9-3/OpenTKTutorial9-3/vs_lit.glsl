#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec2 texcoord;

out vec3 v_norm;
out vec3 v_pos;
out vec2 f_texcoord;

uniform mat4 modelview;
uniform mat4 model;
uniform mat4 view;

void
main()
{
	gl_Position = modelview * vec4(vPosition, 1.0);
	f_texcoord = texcoord;

	mat3 normMatrix = transpose(inverse(mat3(model)));
	v_norm = normMatrix * vNormal;
	v_pos = (model * vec4(vPosition, 1.0)).xyz;
}