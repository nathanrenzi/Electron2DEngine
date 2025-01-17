#shader vertex
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aVertexColor;

out vec2 texCoord;
out vec4 vertexColor;

uniform mat4 projection;
uniform mat4 model;

void main()
{
    texCoord = aTexCoord;
    vertexColor = aVertexColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 vertexColor;

uniform vec4 mainColor;
uniform sampler2D mainTextureSampler;

void main()
{
    vec4 res = texture(mainTextureSampler, texCoord) * vertexColor * mainColor;
    FragColor = vec4(pow(res.r, 1.0 / 2.2), pow(res.g, 1.0 / 2.2), pow(res.b, 1.0 / 2.2), res.w);
}