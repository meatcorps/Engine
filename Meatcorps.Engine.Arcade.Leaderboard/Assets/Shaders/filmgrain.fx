#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float time;
uniform float strength;

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main()
{
    vec4 color = texture(texture0, fragTexCoord);
    float noise = rand(fragTexCoord * resolution + time);
    color.rgb += (noise - 0.5) * strength;
    finalColor = vec4(color.rgb, 1.0);
}