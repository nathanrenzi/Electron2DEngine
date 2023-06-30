#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aColor;
layout (location = 3) in float aIndex;
out vec2 texCoord;
out vec4 vertexColor;
out float texIndex;
    
uniform mat4 projection;
uniform mat4 model;

void main() 
{
    texCoord = aTexCoord;
    texIndex = aIndex;
    vertexColor = aColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;
in vec2 texCoord;
in vec4 vertexColor;
in float texIndex;

uniform sampler2D u_Texture[3];

void main() 
{
    int index = int(texIndex);
    FragColor = texture(u_Texture[index], texCoord) * vertexColor;
}