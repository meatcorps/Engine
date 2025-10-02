#version 330
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;   // source (scene)
uniform float threshold;      // 0..1
uniform float knee;           // soft knee amount (0..1)

void main() {
    vec3 c = texture(texture0, fragTexCoord).rgb;
    float luma = max(max(c.r, c.g), c.b); // cheap “max channel” threshold
    float soft = clamp((luma - threshold) / max(knee, 1e-5), 0.0, 1.0);
    float mask = max(step(threshold, luma), soft);
    finalColor = vec4(c * mask, 1.0);
}