#shader vertex
#version 330 core
layout (location = 0) in vec2 aVertex;
layout (location = 1) in vec2 aTexCoord;
out vec2 texCoord;

uniform mat4 projection;
uniform mat4 uiMatrix;
uniform mat4 model;

void main() 
{
    texCoord = aTexCoord;
    gl_Position = projection * uiMatrix * model * vec4(aVertex.xy, 0.0, 1.0);
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
    float textA = texture(mainTextureSampler, texCoord).r;
    float outlineA = texture(mainTextureSampler, texCoord).g;
    FragColor = vec4(mainColor.rgb, textA * mainColor.a); // + vec4(outlineColor.rgb, outlineA * outlineColor.a)
}