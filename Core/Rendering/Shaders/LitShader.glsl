#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 texColor;
out vec4 position;
    
uniform vec4 mainColor;

uniform mat4 projection;
uniform mat4 model;

void main() 
{
    texCoord = aTexCoord;
    texColor = mainColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);

    position = model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 texColor;
in vec4 position;

uniform sampler2D mainTextureSampler;

struct PointLight {    
    vec2 position;  
    float radius;
    float intensity;
    vec3 color;
};  

#define MAX_POINT_LIGHTS 256  
uniform PointLight pointLights[MAX_POINT_LIGHTS];

vec3 CalcPointLight(PointLight light, vec2 fragPos)
{
    float distance = length(light.position - fragPos);
    //float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    float attenuation = 1.0 / (1 + 0 * distance + 1 * (distance * distance));

    return light.color * light.intensity * attenuation;
} 

void main() 
{
    vec3 objectColor = vec3(texture(mainTextureSampler, texCoord) * texColor);

    vec3 result = vec3(0.0); // Add ambient here
    for(int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        result += CalcPointLight(pointLights[i], position.xy);
    }

    FragColor = vec4(pointLights[0].intensity * vec3(1.0), 1.0); //objectColor * result
}