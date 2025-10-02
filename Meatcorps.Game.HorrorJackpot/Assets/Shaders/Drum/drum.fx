#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texBase; // unit 0
uniform sampler2D texGlow; // unit 1

// --- Placement (normalized to the drawn quad) ---
uniform vec2 uvOffset;   // top-left of the drum area (0..1, 0..1)
uniform vec2 uvSize;     // width/height of the drum area (0..1, 0..1)

// --- Drum controls ---
uniform float rotation01;     // 0..1 vertical roll
uniform float bandCount;      // # of horizontal belts inside the area (e.g., 2)
uniform float uScale;         // >1 reads more texture across X (fixes “×2 wide”)
uniform float uAnchor;        // 0..1 (0=left, 0.5=center, 1=right)
uniform float uOffset;        // small wrap offset after scaling X

// --- Visibility (front / back slice of the reel texture) ---
uniform float vVisibleFrac;   // fraction of texture used vertically (0.5 = front half)
uniform float vVisibleOffset; // which slice to use (0.0..1.0), 0.0 front, 0.5 back

// --- Shading ---
uniform float glow01;         // 0..1 overall glow amount
uniform float tbDarkness;     // 0..1 min brightness at very top/bottom (0=black)
uniform float tbPower;        // falloff exponent for top/bottom fade (2.0 = smooth)

// --- Glow blending ---
uniform float glowMode;       // 0=Add, 1=Screen, 2=SoftAdd
uniform float glowMaskBase;   // 0/1 multiply glow by base alpha if 1
uniform vec3  glowTint;       // RGB tint for glow

const float PI = 3.14159265358979323846;

void main()
{
    // Map current pixel to local [0..1] area for the drum
    vec2 local = (fragTexCoord - uvOffset) / max(uvSize, vec2(0.0001));
    if (local.x < 0.0 || local.x > 1.0 || local.y < 0.0 || local.y > 1.0) {
        discard;
    }

    // -------- Horizontal: scale about anchor, then wrap (no horizontal warp) --------
    float u = (local.x - uAnchor) * max(uScale, 0.0001) + uAnchor;
    u = fract(u + uOffset);

    // -------- Vertical: per-band cylindrical bend (front hemisphere) --------
    float bc    = max(bandCount, 0.0001);
    float yBand = fract(local.y * bc);                 // 0..1 inside current belt
    float yN    = clamp(yBand * 2.0 - 1.0, -1.0, 1.0); // -1..1
    float angle = asin(yN);                            // [-pi/2..+pi/2]
    float vCyl  = (angle + (PI * 0.5)) / PI;          // 0..1 (front side)

    // Show only a vertical slice of the texture (front/back half), still roll & wrap
    float v = vVisibleOffset + vCyl * clamp(vVisibleFrac, 0.0001, 1.0);
    v = fract(v + rotation01);

    vec2 uv = vec2(u, v);

    // -------- Sample base --------
    vec4 baseCol = texture(texBase, uv);

    // Cylinder edge shading (brighter at band center)
    float edgeShade = 0.65 + 0.35 * cos(angle);

    // Extra top/bottom fade for stronger 3D look
    float fade    = 1.0 - pow(abs(yN), max(tbPower, 0.0001)); // 1 in middle, 0 at extremes
    float tbShade = mix(clamp(tbDarkness, 0.0, 1.0), 1.0, fade);

    baseCol.rgb *= edgeShade * tbShade;

    // -------- Glow (respect PNG alpha) --------
    vec4 glowCol = texture(texGlow, uv);
    vec3 glowRgb = glowCol.rgb * glowCol.a; // use glow texture alpha
    glowRgb *= mix(1.0, baseCol.a, clamp(glowMaskBase, 0.0, 1.0)); // optional mask by base alpha
    glowRgb *= glowTint * glow01;

    // Blend glow
    vec3 outRgb;
    if (glowMode < 0.5) {
        // Add
        outRgb = baseCol.rgb + glowRgb;
    } else if (glowMode < 1.5) {
        // Screen
        outRgb = 1.0 - (1.0 - baseCol.rgb) * (1.0 - glowRgb);
    } else {
        // Soft Add
        outRgb = baseCol.rgb + glowRgb - baseCol.rgb * glowRgb;
    }

    finalColor = vec4(outRgb, baseCol.a) * fragColor;
}