#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aIndex;

out float index;
out vec2 texCoord;
out vec4 vertexColor;
    
uniform vec4 mainColor;

uniform mat4 projection;
uniform mat4 model;

void main() 
{
    index = aIndex;
    texCoord = aTexCoord;
    vertexColor = mainColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in float index;
in vec2 texCoord;
in vec4 vertexColor;

uniform float totalLayers;
uniform sampler2D mainTextureSampler;
uniform sampler2DArray mainTextureArraySampler;

void main() 
{
    vec4 res;

    // Replace with more elegant solution in the future
    if(totalLayers == -1)
    {
        // Texture
        res = texture(mainTextureSampler, texCoord) * vertexColor;
    }
    else
    {
        // Texture Array
        float layer = max(0, min(totalLayers - 1, floor(index + 0.5)));
        res = texture(mainTextureArraySampler, vec3(texCoord, layer)) * vertexColor;
    }

    FragColor = vec4(pow(res.r, 1.0 / 2.2), pow(res.g, 1.0 / 2.2), pow(res.b, 1.0 / 2.2), res.w);
}