#version 330

in vec2 fragTexCoord;
out vec4 finalColor;

uniform sampler2D texture0;
uniform vec3 palette[8]; // Up to 8 colors

void main()
{
    vec3 color = texture(texture0, fragTexCoord).rgb;
    float minDist = 10.0;
    vec3 closest = palette[0];

    for (int i = 0; i < 8; i++)
    {
        float dist = distance(color, palette[i]);
        if (dist < minDist)
        {
            minDist = dist;
            closest = palette[i];
        }
    }

    finalColor = vec4(closest, 1.0);
}