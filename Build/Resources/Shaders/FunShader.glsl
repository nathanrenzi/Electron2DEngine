#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 texCoord;
out vec4 vertexColor;
out float sTime;
    
uniform float time; // Set this for a fun morphing shape
uniform vec4 mainColor;

uniform mat4 projection;
uniform mat4 model;

void main() 
{
    sTime = time;

    texCoord = aTexCoord;
    vertexColor = mainColor;
    gl_Position = projection * model * vec4(aPosition.xy, 0.0, 1.0);
}

#shader fragment
#version 330 core
out vec4 FragColor;

in vec2 texCoord;
in vec4 vertexColor;
in float sTime;

uniform sampler2D mainTextureSampler;

vec3 palette(float t)
{
    vec3 a = vec3(0.892, 0.725, 0.000);
    vec3 b = vec3(0.878, 0.278, 0.725);
    vec3 c = vec3(0.332, 0.518, 0.545);
    vec3 d = vec3(2.440, 5.043, 0.732);

    return a + b*cos( 6.28318*(c*t*d) );
}

void main() 
{
    vec2 uv = texCoord * 2;
    vec2 uv0 = uv - 1;
    vec3 finalColor = vec3(0.0);

    for (float i = 0.0; i < 3.0; i++)
    {
        uv = fract(uv * 1.5) - 0.5;

        float d = length(uv) * exp(-length(uv0));

        vec3 col = palette(length(uv0) + i*0.4 + sTime*0.4);

        d = sin(d*8.0 + sTime)/8.0;
        d = abs(d);

        d = pow(0.01 / d, 1.2);

        finalColor += col * d;
    }

    FragColor = vec4(finalColor, 1.0);
}