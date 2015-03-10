#version 330

in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D mytexture;
 
void main(void) {
	outputColor = texture2D(mytexture, f_texcoord);
}