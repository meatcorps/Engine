#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;

void main()
{
    vec4 texColor = texture(texture0, fragTexCoord);
    float gray = dot(texColor.rgb, vec3(0.299, 0.587, 0.114));
    finalColor = vec4(gray, gray, gray, texColor.a) * fragColor;
}