#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

uniform float xPos;

out vec3 vertColor;
out vec4 vertPos;

void main()
{
    gl_Position = vec4(aPos.x + xPos, aPos.y, aPos.z, 1.0);
    vertColor = aColor;
    vertPos = gl_Position;
}
