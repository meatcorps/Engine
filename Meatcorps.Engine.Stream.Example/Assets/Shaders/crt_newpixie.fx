#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
out vec4 finalColor;

uniform sampler2D texture0;   // main scene
uniform sampler2D frameTex;   // optional bezel/frame overlay

uniform vec2  resolution;     // target size (pixels)
uniform float time;           // seconds
uniform float curvature;      // ~2.0
uniform float wiggleToggle;   // 0/1
uniform float scanroll;       // 0..1
uniform float vignette;       // 0..1
uniform float ghosting;       // 0..2
uniform float useFrame;       // 0/1

vec3 tsample(sampler2D samp, vec2 tc) {
    tc = tc * vec2(1.025, 0.92) + vec2(-0.0125, 0.04);
    vec3 s = pow(abs(texture(samp, tc).rgb), vec3(2.2)); // <- no 1.0 - tc.y
    return s*vec3(1.25);
}

vec3 filmic(vec3 x){
    x = max(vec3(0.0), x - vec3(0.004));
    return (x*(6.2*x+0.5)) / (x*(6.2*x+1.7) + 0.06);
}

vec2 curve(vec2 uv){
    uv = (uv - 0.5);
    uv *= vec2(0.925, 1.095);
    uv *= curvature;
    uv.x *= 1.0 + pow(abs(uv.y)/4.0, 2.0);
    uv.y *= 1.0 + pow(abs(uv.x)/3.0, 2.0);
    uv /= curvature;
    uv += 0.5;
    uv = uv * 0.92 + 0.04;
    return uv;
}

float rand(vec2 co){ return fract(sin(dot(co, vec2(12.9898,78.233))) * 43758.5453); }

// Cheap built-in blur (boxy gauss)
vec3 sampleBlurred(sampler2D samp, vec2 uv){
    vec2 px = 1.0 / max(resolution, vec2(1.0)); // guard
    vec3 c = vec3(0.0);
    c += texture(samp, uv + px*vec2(-2,  0)).rgb * 0.06;
    c += texture(samp, uv + px*vec2(-1,  0)).rgb * 0.12;
    c += texture(samp, uv).rgb                * 0.16;
    c += texture(samp, uv + px*vec2( 1,  0)).rgb * 0.12;
    c += texture(samp, uv + px*vec2( 2,  0)).rgb * 0.06;
    c += texture(samp, uv + px*vec2( 0, -2)).rgb * 0.06;
    c += texture(samp, uv + px*vec2( 0, -1)).rgb * 0.12;
    c += texture(samp, uv + px*vec2( 0,  1)).rgb * 0.12;
    c += texture(samp, uv + px*vec2( 0,  2)).rgb * 0.06;
    return c;
}

void main(){
    vec2 uv = fragTexCoord;
    float t = time;

    vec2 curved = mix(curve(uv), uv, 0.4);
    float scale = -0.101;
    vec2 scuv = curved * (1.0 - scale) + scale * 0.5 + vec2(0.003, -0.001);
    uv = scuv;

    float x = wiggleToggle
        * sin(0.1*t + curved.y*13.0)
        * sin(0.23*t + curved.y*19.0)
        * sin(0.3 + 0.11*t + curved.y*23.0) * 0.0012;

    float o = sin(gl_FragCoord.y * 1.5) / resolution.x;
    x += o * 0.25;

    vec3 col;
    col.r = tsample(texture0, vec2(x + scuv.x + 0.0009, scuv.y + 0.0009)).r + 0.02;
    col.g = tsample(texture0, vec2(x + scuv.x + 0.0000, scuv.y - 0.0011)).g + 0.02;
    col.b = tsample(texture0, vec2(x + scuv.x - 0.0015, scuv.y + 0.0000)).b + 0.02;

    float i = clamp(col.r*0.299 + col.g*0.587 + col.b*0.114, 0.0, 1.0);
    i = pow(1.0 - pow(i, 2.0), 1.0);
    i = (1.0 - i) * 0.85 + 0.15;

    float ghs = 0.15 * ghosting;
    vec3 r = sampleBlurred(texture0, vec2(
        x-0.014, -0.027)*0.85
        + 0.007*vec2(0.35*sin(1.0/7.0 + 15.0*curved.y + 0.9*t),
                     0.35*sin(2.0/7.0 + 10.0*curved.y + 1.37*t))
        + vec2(scuv.x+0.001, scuv.y+0.001)) * vec3(0.5,0.25,0.25);

    vec3 g = sampleBlurred(texture0, vec2(
        x-0.019, -0.020)*0.85
        + 0.007*vec2(0.35*cos(1.0/9.0 + 15.0*curved.y + 0.5*t),
                     0.35*sin(2.0/9.0 + 10.0*curved.y + 1.50*t))
        + vec2(scuv.x+0.000, scuv.y-0.002)) * vec3(0.25,0.5,0.25);

    vec3 b = sampleBlurred(texture0, vec2(
        x-0.017, -0.003)*0.85
        + 0.007*vec2(0.35*sin(2.0/3.0 + 15.0*curved.y + 0.7*t),
                     0.35*cos(2.0/3.0 + 10.0*curved.y + 1.63*t))
        + vec2(scuv.x-0.002, scuv.y+0.000)) * vec3(0.25,0.25,0.5);

    col += vec3(ghs*(1.0-0.299)) * pow(clamp(3.0*r, 0.0, 1.0), vec3(2.0)) * vec3(i);
    col += vec3(ghs*(1.0-0.587)) * pow(clamp(3.0*g, 0.0, 1.0), vec3(2.0)) * vec3(i);
    col += vec3(ghs*(1.0-0.114)) * pow(clamp(3.0*b, 0.0, 1.0), vec3(2.0)) * vec3(i);

    col *= vec3(0.95,1.05,0.95);
    col = clamp(col*1.3 + 0.75*col*col + 1.25*col*col*col*col*col, 0.0, 10.0);

    float vig = ((1.0 - 0.99*vignette) + 16.0*curved.x*curved.y*(1.0-curved.x)*(1.0-curved.y));
    vig = 1.3*pow(vig, 0.5);
    col *= vig;

    float scans = clamp(0.35 + 0.18 * sin(6.0 * (t*scanroll) - curved.y * resolution.y * 1.5), 0.0, 1.0);
    col *= vec3(pow(scans, 0.9)); // <- fixed

    col *= 1.0 - 0.23 * clamp(mod(gl_FragCoord.x, 3.0)/2.0, 0.0, 1.0);

    col = filmic(col);
    vec2 seed = curved * resolution;
    col -= 0.015 * pow(vec3(rand(seed+t), rand(seed+2.0*t), rand(seed+3.0*t)), vec3(1.5));
    col *= (1.0 - 0.004 * (sin(50.0*t + curved.y*2.0)*0.5 + 0.5));

    if (useFrame > 0.5) {
        vec2 uvf = curved;
        vec4 f = texture(frameTex, uvf);
        f.rgb = mix(f.rgb, vec3(0.5), 0.5);
        float fvig = clamp(512.0*uvf.x*uvf.y*(1.0-uvf.x)*(1.0-uvf.y), 0.2, 0.8);
        col = mix(col, mix(max(col, 0.0), pow(abs(f.rgb), vec3(1.4))*fvig, f.a*f.a), 1.0);
    }

    finalColor = vec4(col, 1.0) * fragColor;
}