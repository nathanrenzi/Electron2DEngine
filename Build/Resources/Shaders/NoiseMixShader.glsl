#shader vertex
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;

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

uniform sampler2D mainTextureSampler;
uniform sampler2D main2TextureSampler;

float rand(vec2 c)
{
	return fract(sin(dot(c.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

float noise(vec2 p, float freq)
{
	float unit = 1920 / freq;
	vec2 ij = floor(p / unit);
	vec2 xy = mod(p, unit) / unit;
	//xy = 3.*xy*xy-2.*xy*xy*xy;
	xy = .5 * (1. - cos(3.14159265 * xy));
	float a = rand((ij + vec2(0., 0.)));
	float b = rand((ij + vec2(1., 0.)));
	float c = rand((ij + vec2(0., 1.)));
	float d = rand((ij + vec2(1., 1.)));
	float x1 = mix(a, b, xy.x);
	float x2 = mix(c, d, xy.x);
	return mix(x1, x2, xy.y);
}

void main() 
{
    float percent = texCoord.x;
	float result = noise(texCoord, 20000) * 0.625;
	result += noise(texCoord, 40000) * 0.25;
	result += noise(texCoord, 80000) * 0.125;
	result += noise(texCoord, 160000) * 0.125 * 0.5;
	result += noise(texCoord, 320000) * 0.125 * 0.25;
	vec4 noiseCol = vec4(result, result, result, 1);
    vec4 res = mix(texture(mainTextureSampler, texCoord) * vertexColor, texture(main2TextureSampler, texCoord) * vertexColor, percent);
	FragColor = noiseCol; //vec4(pow(res.r, 1.0 / 2.2), pow(res.g, 1.0 / 2.2), pow(res.b, 1.0 / 2.2), res.w);
}