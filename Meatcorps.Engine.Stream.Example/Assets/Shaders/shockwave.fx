#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float time;
uniform vec2 center;
uniform float speed;
uniform float size;

void main()
{
    vec2 uv = fragTexCoord;
    vec2 dir = uv - center;
    float dist = length(dir);

    float wave = sin(dist * size - time * speed) * 0.02 / (dist * size + 1.0);
    uv += normalize(dir) * wave;

    finalColor = texture(texture0, uv);
}