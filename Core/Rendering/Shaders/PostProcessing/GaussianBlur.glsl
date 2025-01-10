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
uniform float blurRadius;
uniform int kernelSize;
#define MAX_COEFFICIENT_ARRAY_SIZE 2048
uniform float coeffs[MAX_COEFFICIENT_ARRAY_SIZE];


void main()
{
    vec4 res = vec4(0);
    int N = kernelSize * 2 + 1;
    for (int i = 0; i < N; ++i)
    {
        vec2 tc = texCoord + vec2(1, 0) * float(i - kernelSize) * blurRadius * 0.0005;
        res += coeffs[i] * texture(frameBufferTexture, tc);
    }
    FragColor = res;
}