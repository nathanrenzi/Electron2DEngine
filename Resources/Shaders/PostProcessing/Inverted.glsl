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

uniform sampler2D frameBufferTexture;

void main()
{
    vec4 res = texture(frameBufferTexture, texCoord);
    FragColor = vec4(1 - res.r, 1 - res.g, 1 - res.b, 1.0f);
}