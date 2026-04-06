#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 vertexColor;
    
uniform vec4 mainColor;

uniform mat4 projection;
uniform mat4 model;

void main() 
{
    texCoord = aTexCoord;
    vertexColor = mainColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 vertexColor;

uniform sampler2D mainTextureSampler;

void main() 
{
    vec4 res = texture(mainTextureSampler, texCoord) * vertexColor;
    FragColor = res;
}