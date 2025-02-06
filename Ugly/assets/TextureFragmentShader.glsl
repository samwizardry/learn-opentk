#version 330 core

uniform sampler2D mainTexture;
uniform sampler2D texture2;
uniform float mixFactor;

in vec3 VertColor;
in vec2 TexCoord;

out vec4 FragColor;

void main()
{
    FragColor = mix(texture(mainTexture, TexCoord), texture(texture2, TexCoord), mixFactor);//* vec4(VertColor, 1.0);
}
