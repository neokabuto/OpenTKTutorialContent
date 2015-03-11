#version 330

in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D mytexture;
 
void main(void) {
	outputColor = vec4(1,1,1,1);
}