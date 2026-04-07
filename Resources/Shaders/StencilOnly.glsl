#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

uniform mat4 projection;
uniform mat4 uiMatrix;
uniform mat4 model;

void main() 
{
    gl_Position = projection * uiMatrix * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

void main() 
{
    FragColor = vec4(1.0);
}