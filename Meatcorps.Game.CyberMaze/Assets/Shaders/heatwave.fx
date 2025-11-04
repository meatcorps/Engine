#version 330

uniform sampler2D texture0;
uniform float time;
uniform vec2 resolution;

    in vec2 fragTexCoord;
    out vec4 fragColor;

void main()
{
    vec2 uv = fragTexCoord;

    // Apply distortion centered around the middle of the screen
    float strength = 0.01;
    float wave = sin(uv.y * 30.0 + time * 5.0) * strength;
    uv.x += wave;

    wave = cos(uv.x * 30.0 + time * 5.0) * strength;
    uv.y += wave;

    fragColor = texture(texture0, uv);
}