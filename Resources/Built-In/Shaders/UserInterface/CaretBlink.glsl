#shader vertex
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

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

uniform float time = 0;
uniform float startTime = 0;
uniform sampler2D mainTextureSampler;

float getAlpha()
{
    return pow(sin(3.14159265 * ((time - startTime + 0.2) - floor(time - startTime)) * 2), 0.1);
}

void main()
{
    FragColor = vec4((texture(mainTextureSampler, texCoord) * vertexColor).rgb, getAlpha());
}