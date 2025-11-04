#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;      // scene (PLEASE set point/nearest filtering on sprite textures)
uniform sampler2D paletteTex;    // N x 1 palette (point-filtered)
uniform int  paletteSize;        // number of valid entries
uniform int  usePerceptual;      // 0 = plain RGB distance, 1 = linear+weighted

// Dithering controls
uniform float ditherStrength;    // 0 disables; typical 1/255..2/255
uniform float ditherScale;       // 1..4 = pixels per Bayer cell
uniform vec2  ditherOffset;      // temporal offset (0..4)

// Exact-match snap threshold (in sRGB 0..1); e.g. 0.5/255 ~= one LSB wiggle
uniform float exactEpsilon;

// --- perceptual-ish distance in linear space ---
vec3 srgbToLinear(vec3 c){ return pow(c, vec3(2.2)); }

float distRGB(vec3 a, vec3 b) {       // simple Euclidean in sRGB
    vec3 d = a - b;
    return dot(d, d);
}

float distPerceptual(vec3 a, vec3 b){ // weighted Euclidean in linear
    vec3 d = srgbToLinear(a) - srgbToLinear(b);
    // Rec.709-ish weights; no sqrt needed (monotonic)
    return dot(d, vec3(0.2126, 0.7152, 0.0722));
}

float colorDist(vec3 a, vec3 b){
    return (usePerceptual != 0) ? distPerceptual(a,b) : distRGB(a,b);
}

// --- 4x4 Bayer ---
float bayer4x4(vec2 p){
    int x = int(mod(p.x, 4.0));
    int y = int(mod(p.y, 4.0));
    int i = x + y*4;
    float m[16] = float[](
        0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
        12.0/16.0, 4.0/16.0, 14.0/16.0,  6.0/16.0,
        3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
        15.0/16.0, 7.0/16.0, 13.0/16.0,  5.0/16.0
    );
    return m[i];
}

void main(){
    // sample source
    vec3 src = texture(texture0, fragTexCoord).rgb;

    int count = max(paletteSize, 1);

    // ---- 1) Early exact-match snap (in sRGB) ----
    // If any palette entry is within exactEpsilon of src, return it immediately (no dithering).
    for (int i = 0; i < count; i++) {
        vec3 p = texture(paletteTex, vec2((float(i)+0.5)/float(count), 0.5)).rgb;
        vec3 diff = abs(src - p);
        if (max(diff.r, max(diff.g, diff.b)) <= exactEpsilon) {
            finalColor = vec4(p, 1.0) * fragColor;
            return;
        }
    }

    // ---- 2) Optional ordered dithering BEFORE quantization ----
    if (ditherStrength > 0.0) {
        vec2 dp = (gl_FragCoord.xy / max(ditherScale, 1.0)) + ditherOffset;
        float d = (bayer4x4(dp) - 0.5) * 2.0 * ditherStrength;
        src = clamp(src + vec3(d), 0.0, 1.0);
    }

    // ---- 3) Nearest palette color ----
    float best = 1e9;
    vec3 chosen = src;

    for (int i = 0; i < count; i++) {
        float u = (float(i) + 0.5) / float(count);
        vec3 p = texture(paletteTex, vec2(u, 0.5)).rgb;
        float dist = colorDist(src, p);
        if (dist < best) {
            best = dist;
            chosen = p;
        }
    }

    finalColor = vec4(chosen, 1.0) * fragColor;
}