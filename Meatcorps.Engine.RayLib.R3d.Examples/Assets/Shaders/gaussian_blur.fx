#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;

// Size in pixels of the texture being blurred (NOT window size!)
uniform vec2 resolution;

// (1, 0) for horizontal, (0, 1) for vertical
uniform vec2 direction;

// Blur spread / radius factor.
// 1.0 = base blur, 2.0 = wider, 0.5 = sharper
uniform float spread;

// Radius of the kernel (4 → 9 taps)
const int RADIUS = 8;

void main()
{
    // One texel in the chosen direction
    vec2 texel = direction / resolution;

    // Base sigma – you can tweak this
    float baseSigma = 1.0;

    // Effective sigma scales with spread
    float sigma = baseSigma + spread * 3.0;
    float twoSigma2 = 2.0 * sigma * sigma;

    vec4 sum = vec4(0.0);
    float weightSum = 0.0;

    for (int i = -RADIUS; i <= RADIUS; i++)
    {
        float x = float(i);

        // Gaussian weight for this offset
        float w = exp(-(x * x) / twoSigma2);

        vec2 uv = fragTexCoord + x * texel;
        uv = clamp(uv, vec2(0.0), vec2(1.0));

        vec4 sampleColor = texture(texture0, uv);

        sum += sampleColor * w;
        weightSum += w;
    }

    // Normalize so total weight = 1
    sum /= weightSum;

    finalColor = sum;
}