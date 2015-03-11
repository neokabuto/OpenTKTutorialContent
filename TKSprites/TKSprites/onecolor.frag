#version 330

in vec2 f_texcoord;
out vec4 outputColor;

uniform sampler2D mytexture;
 
void main(void) {
	outputColor = vec4(texture2D(mytexture, vec2(0.1,0.1)).rgb, texture2D(mytexture, f_texcoord).a);
}