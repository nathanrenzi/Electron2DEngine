#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec4 aColor;
out vec4 vertexColor;
    
uniform mat4 projection;
uniform mat4 model;

void main() 
{
    vertexColor = aColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
in vec4 vertexColor;
out vec4 FragColor;

void main() 
{
    FragColor = pow(vertexColor, 2.2);
}