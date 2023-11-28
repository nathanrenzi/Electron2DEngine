#shader vertex
#version 330 core
layout (location = 0) in vec4 vertex;
out vec2 texCoord;

uniform mat4 projection;

void main() 
{
    texCoord = vertex.zw;
    gl_Position = projection * vec4(vertex.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec4 mainColor;
uniform vec4 outlineColor;
uniform sampler2D mainTextureSampler;

void main()
{
    vec4 textC = vec4(1.0, 1.0, 1.0, texture(mainTextureSampler, texCoord).r);
    vec4 outlineC = vec4(1.0, 1.0, 1.0, texture(mainTextureSampler, texCoord).g);
    //FragColor = (textC * mainColor) + (outlineC * outlineColor);
    FragColor = (textC * mainColor);
}