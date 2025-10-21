#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float intensity;
uniform float threshold;

void main()
{
    vec4 base = texture(texture0, fragTexCoord);

    vec4 bloom = vec4(0.0);
    float offsets[5] = float[]( -2.0, -1.0, 0.0, 1.0, 2.0 );

    for (int x = 0; x < 5; x++)
    for (int y = 0; y < 5; y++)
    {
        vec2 offset = vec2(offsets[x], offsets[y]) / resolution;
        vec4 sample = texture(texture0, fragTexCoord + offset);
        if (max(sample.r, max(sample.g, sample.b)) > threshold)
            bloom += sample;
    }

    bloom /= 25.0;
    finalColor = base + bloom * intensity;
}