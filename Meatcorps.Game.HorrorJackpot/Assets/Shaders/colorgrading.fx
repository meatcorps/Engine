#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform sampler2D lutTexture;

vec3 sampleAs3DTexture(sampler2D tex, vec3 uv)
{
    float blueOffset = floor(uv.b * 15.0);
    vec2 quadPos;
    quadPos.x = (uv.r * 15.0 + blueOffset) / 256.0;
    quadPos.y = uv.g / 16.0;
    return texture(tex, quadPos).rgb;
}

void main()
{
    vec4 color = texture(texture0, fragTexCoord);
    finalColor = vec4(sampleAs3DTexture(lutTexture, color.rgb), color.a);
}