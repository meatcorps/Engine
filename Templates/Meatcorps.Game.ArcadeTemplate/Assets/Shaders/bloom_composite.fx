#version 330
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;   // blurred bright
uniform sampler2D sceneTex;   // original scene
uniform float intensity;      // bloom strength

void main() {
    vec3 scene = texture(sceneTex, fragTexCoord).rgb;
    vec3 blur  = texture(texture0, fragTexCoord).rgb;
    vec3 outc  = scene + blur * intensity;
    finalColor = vec4(outc, 1.0);
}