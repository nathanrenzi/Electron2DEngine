#shader vertex
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

void main()
{
    texCoord = aTexCoord;
    gl_Position = vec4(aPosition.xy, 0.0f, 1.0f);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;

uniform vec2 resolution;
uniform vec4 color;
uniform float softness;
uniform float radius;
uniform float roundness;
uniform sampler2D frameBufferTexture;

float sampleVignette()
{
    vec2 position = texCoord - vec2(0.5);        
    float distance = length(position * mix(vec2(1), vec2(resolution.x/resolution.y, 1.0), roundness));

    return 1 - smoothstep(radius, radius - softness, distance);
}

void main()
{
    vec4 res = texture(frameBufferTexture, texCoord);
    FragColor = mix(res, color, sampleVignette());
}