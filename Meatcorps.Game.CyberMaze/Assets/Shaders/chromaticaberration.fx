#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float amount;

void main()
{
    vec2 offset = amount / resolution;
    float r = texture(texture0, fragTexCoord + offset).r;
    float g = texture(texture0, fragTexCoord).g;
    float b = texture(texture0, fragTexCoord - offset).b;
    finalColor = vec4(r, g, b, 1.0);
}