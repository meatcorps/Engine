#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec3 colorA;
uniform vec3 colorB;
uniform vec3 colorC;
uniform vec3 colorD;

void main()
{
    vec4 tex = texture(texture0, fragTexCoord);
    float brightness = (tex.r + tex.g + tex.b) / 3.0;

    if (brightness < 0.25) tex.rgb = colorA;
    else if (brightness < 0.5) tex.rgb = colorB;
    else if (brightness < 0.75) tex.rgb = colorC;
    else tex.rgb = colorD;

    finalColor = tex;
}