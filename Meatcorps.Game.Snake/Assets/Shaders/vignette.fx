#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float strength; 
uniform float radius;

void main()
{
    vec2 uv = fragTexCoord;
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(uv, center);
    float vignette = smoothstep(radius, radius - 0.5, dist * strength);
    vec4 texColor = texture(texture0, uv);
    finalColor = vec4(texColor.rgb * vignette, texColor.a) * fragColor;
}