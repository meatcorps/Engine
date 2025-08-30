#version 330

uniform sampler2D texture0;
uniform vec2 resolution;
uniform float time;

// Tweaks
uniform float distortion;
uniform float scanlineIntensity;
uniform float flickerStrength;
uniform float chromaticAberration;
uniform float noiseStrength;
uniform float vignetteStrength;
uniform float vignetteRadius;

    in vec2 fragTexCoord;
    out vec4 finalColor;

float rand(vec2 co)
{
    return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

void main()
{
    vec2 uv = fragTexCoord;
    vec2 centered = uv * 2.0 - 1.0;
    float r2 = dot(centered, centered);

    // Barrel distortion
    vec2 barrel = centered * (1.0 + distortion * r2);
    vec2 distortedUV = (barrel + 1.0) * 0.5;

    // Chromatic aberration
    vec3 color;
    color.r = texture(texture0, distortedUV + vec2(chromaticAberration, 0.0)).r;
    color.g = texture(texture0, distortedUV).g;
    color.b = texture(texture0, distortedUV - vec2(chromaticAberration, 0.0)).b;

    // Scanlines
    float scanline = sin((distortedUV.y + time * 2.0) * resolution.y * 1.5) * scanlineIntensity;
    color *= 1.0 - scanline;

    // Flicker
    float flicker = flickerStrength * sin(time * 120.0);
    color += flicker;

    // Noise
    float noise = (rand(distortedUV * time) - 0.5) * noiseStrength;
    color += noise;

    // Vignette
    float vignette = smoothstep(0.0, vignetteRadius, r2);
    color *= mix(1.0, vignetteStrength, vignette);
    
    // Highlight reflection (optional)
    float highlight = pow(1.0 - length(centered), 10.0);
    color += vec3(0.05) * highlight;

    // Clamp outside
    if (distortedUV.x < 0.0 || distortedUV.x > 1.0 || distortedUV.y < 0.0 || distortedUV.y > 1.0)
        color = vec3(0.0);

    finalColor = vec4(clamp(color, 0.0, 1.0), 1.0);
}