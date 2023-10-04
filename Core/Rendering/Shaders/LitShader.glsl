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
uniform sampler2D normalTextureSampler;

struct PointLight {    
    vec2 position;  
    float radius;
    float intensity;
    vec3 color;
};  

#define MAX_POINT_LIGHTS 16  
uniform PointLight pointLights[MAX_POINT_LIGHTS];

vec3 CalcPointLight(PointLight light, vec2 fragPos, vec3 normal)
{
    vec3 lightDirection = vec3(normalize(light.position - fragPos), 0.0); // Could raise or lower height of light here?? Or if a height value is added replace 0.0 with z position
    float distance = length(light.position - fragPos);
    //float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    float attenuation = 1.0 / (1 + 0 * distance + 3/(light.radius * light.radius) * (distance * distance));

    float diffuse = max(dot(normal, lightDirection), 0.0);

    return light.color * light.intensity * attenuation * diffuse;
} 

void main() 
{
    vec3 normal = vec3(texture(normalTextureSampler, texCoord));
    normal = normalize(normal * 2.0 - 1.0);

    vec3 objectColor = vec3(texture(mainTextureSampler, texCoord) * texColor);

    vec3 result = vec3(0.0); // Add ambient here
    for(int i = 0; i < MAX_POINT_LIGHTS; i++)
    {
        result += CalcPointLight(pointLights[i], position.xy, normal);
    }

    FragColor = vec4(objectColor * result, 1.0);
}