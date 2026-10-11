#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float intensity;

void main()
{
    vec4 color = texture(texture0, fragTexCoord);
    float scanline = sin(fragTexCoord.y * resolution.y * 3.14159) * 0.5 + 0.5;
    color.rgb *= mix(1.0, scanline, intensity);
    finalColor = color;
}