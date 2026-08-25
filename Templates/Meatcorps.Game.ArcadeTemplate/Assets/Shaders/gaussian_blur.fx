#version 330
in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;   // target size
uniform vec2 direction;    // (1,0) for horizontal, (0,1) for vertical
uniform float spread;

// 9-tap Gaussian weights (sigma ~ 2.0)
const float w[9] = float[](0.05, 0.09, 0.12, 0.15, 0.18, 0.15, 0.12, 0.09, 0.05);

void main() {
    vec2 texel = direction * (spread / resolution);
    vec3 sum = vec3(0.0);
    int k = 0;
    for (int i = -4; i <= 4; i++) {
        vec2 uv = fragTexCoord + float(i) * texel;
        sum += texture(texture0, uv).rgb * w[k++];
    }
    finalColor = vec4(sum, 1.0);
}