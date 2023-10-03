#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 vertexColor;
out vec4 position;
    
uniform vec4 mainColor;

uniform mat4 projection;
uniform mat4 model;

void main() 
{
    texCoord = aTexCoord;
    vertexColor = mainColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);

    position = model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 vertexColor;
in vec4 position;

uniform vec2 lightPosition;
uniform sampler2D mainTextureSampler;

void main() 
{
    vec4 objectColor = texture(mainTextureSampler, texCoord) * vertexColor;
    vec4 lightColor = vec4(1, 0.9, 0.8, 1.0);

    float ambientStrength = 0.3;
    float lightDistance = 400;
    float lightIntensity = 1.5;
    float d = distance(position.xy, lightPosition);
    float p = pow(1 - (clamp(d / lightDistance, 0, 1)), 2);

    vec4 ambient = ambientStrength * lightColor;
    vec4 result = objectColor * clamp((lightColor * lightIntensity) * p, ambient, vec4(1.0));

    FragColor = result;
}