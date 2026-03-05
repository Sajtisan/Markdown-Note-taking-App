namespace MarkdownNotesClient.Controls;

public static class ThemeManager
{
    // THEME 1: DARK - https://www.shadertoy.com/view/7ldGWf
    public static string Theme_Dark = @"
    uniform float u_time;
    uniform vec2 u_resolution;

    const vec3 COLOR = vec3(0.42, 0.40, 0.47);
    const vec3 BG = vec3(0.0, 0.0, 0.0);
    const float ZOOM = 3.0;
    const int OCTAVES = 4;
    const float INTENSITY = 2.0;

    float random(vec2 st) {
        return fract(sin(dot(st.xy, vec2(12.9818,79.279)))*43758.5453123);
    }

    vec2 random2(vec2 st){
        st = vec2(dot(st,vec2(127.1,311.7)), dot(st,vec2(269.5,183.3)));
        return -1.0 + 2.0 * fract(sin(st) * 7.0);
    }

    float noise(vec2 st) {
        vec2 i = floor(st);
        vec2 f = fract(st);

        // smootstep
        vec2 u = f*f*(3.0-2.0*f);

        return mix( mix( dot( random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                         dot( random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                    mix( dot( random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                         dot( random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
    }

    float fractal_brownian_motion(vec2 coord) {
        float value = 0.0;
        float scale = 0.2;
        for (int i = 0; i < 4; i++) {
            value += noise(coord) * scale;
            coord *= 2.0;
            scale *= 0.5;
        }
        return value + 0.2;
    }

    vec4 main(vec2 fragCoord) {
        vec2 st = fragCoord.xy / u_resolution.xy;
        st *= u_resolution.xy / u_resolution.y;    
        vec2 pos = vec2(st * ZOOM);
        
        // Swapped iTime for u_time
        vec2 motion = vec2(fractal_brownian_motion(pos + vec2(u_time * -0.5, u_time * -0.3)));
        float final = fractal_brownian_motion(pos + motion) * INTENSITY;
        
        // Swapped fragColor output for a return statement
        return vec4(mix(BG, COLOR, final), 1.0);
    }";

    // THEME 2: DRACULA - https://www.shadertoy.com/view/llsSzB
    public static string Theme_Dracula = @"
    uniform float u_time;
    uniform vec2 u_resolution;

    float hash(float x) {
        return fract(21654.6512 * sin(385.51 * x));
    }

    float hash(vec2 p) {
        return fract(21654.65155 * sin(35.51 * p.x + 45.51 * p.y));
    }

    float lhash(float x, float y) {
        float h = 0.0;
        for(int i = 0; i < 5; i++) {
            h += (fract(21654.65155 * float(i) * sin(35.51 * x + 45.51 * float(i) * y * (5.0 / float(i))))* 2.0 - 1.0) / 10.0;
        }
        return h / 5.0 + 0.02;
        // SkSL Strictness: Unreachable code causes compilation errors!
        // return (fract(21654.65155 * sin(35.51 * x + 45.51 * y))* 2.0 - 1.0) / 20.0;
    }

    float noise(vec2 p) {
        vec2 fl = floor(p);
        vec2 fr = fract(p);
        
        fr.x = smoothstep(0.0, 1.0, fr.x);
        fr.y = smoothstep(0.0, 1.0, fr.y);
        
        float a = mix(hash(fl + vec2(0.0, 0.0)), hash(fl + vec2(1.0, 0.0)), fr.x);
        float b = mix(hash(fl + vec2(0.0, 1.0)), hash(fl + vec2(1.0, 1.0)), fr.x);
        
        return mix(a, b, fr.y);
    }

    float fbm(vec2 p) {
        float v = 0.0, f = 1.0, a = 0.5;
        for(int i = 0; i < 5; i++) {
            v += noise(p * f) * a;
            f *= 2.0;
            a *= 0.5;
        }
        return v;
    }

    vec4 main(vec2 fragCoord) {
        float time = u_time * 1.0;
        vec2 uv = fragCoord.xy / u_resolution.xy;
        uv = uv * 2.0 - 1.0;
        uv.x *= u_resolution.x / u_resolution.y;    

        float p = fbm(vec2(noise(uv + time / 2.5), noise(uv * 2.0 + cos(time / 2.0) / 2.0)));

        vec3 col = pow(vec3(p), vec3(0.3)) - 0.4;
        col = mix(col, vec3(1.0), 1.0 - smoothstep(0.0, 0.2, pow(1.0 / 2.0, 0.5) - uv.y / 40.0));
        float s = smoothstep(.35, .6, col.x);
        float s2 = smoothstep(.47, .6, col.x);
        float s3 = smoothstep(.51, .6, col.x);
        
        col *= vec3(1.3, .1, 0.1) * s; // add red
        col += vec3(0.3, 0.4, .1) * s2; // add orange
        col += vec3(1.0, 4.0, .1) * s3; // add yellow
        
        col *= 1.5;
        vec4 fragColor = vec4(col, col.r * 0.8);
        fragColor.rgb += 0.05;

        return fragColor;
    }";

    // THEME 3: LIGHT - https://www.shadertoy.com/view/4ltXRr
    public static string Theme_Light = @"
    uniform float u_time;
    uniform vec2 u_resolution;

    // Complex Exponentiation
    vec2 powC(vec2 Z, vec2 W) {
        float arg = atan(Z.y / Z.x);
        float a = exp(-W.y * arg) * pow(dot(Z, Z), W.x / 4.0);
        float b = W.x * arg + 0.5 * W.y * log(dot(Z, Z));
        
        return a * vec2(cos(b), sin(b));    
    } 

    vec2 cmult(vec2 q1, vec2 q2) {
        return vec2(
            q1.x * q2.x - q1.y * q2.y, 
            q1.x * q2.y + q1.y * q2.x
        );
    }

    float smoke(vec2 uv, float param) {
        float frac = 1.0;
        vec2 z = vec2(0.0, 0.0);
        vec2 c = uv;
        
        float timefact = cos(u_time * 0.25 + param * 0.001);
        vec2 constant = powC(cos(c), vec2(33.8 - param * 0.01, 10.8));
        
        for(int i = 0; i < 20; i++) { 
            z = cmult(sin(z - c), cos(z * z + vec2(param)) - c) + timefact * 1.0 * z + constant;
            
            if(length(z) > 2.0) {  
                frac = float(i) / 20.0;
                break;
            }
        }
        
        return frac;
    }

    const int NSMOKES = 40;

    vec4 main(vec2 fragCoord) {
        vec2 uv = fragCoord.xy / u_resolution.xy;
        uv -= vec2(0.5, 0.5);
        uv *= 2.0;
        
        float smokes = 0.0;
        for(int i = 0; i < NSMOKES; i++) {
            smokes += smoke(uv, float(i) * 30.0) * (1.0 / float(NSMOKES));
        }
        
        // INVERT THE COLORS FOR A TRUE LIGHT THEME!
        // `smokes` is normally white smoke. 1.0 - smokes makes the background white and smoke dark.
        // If you want the original black background, change the next line to: 
        // float finalColor = smokes;
        float finalColor = 1.0 - smokes; 

        // We use 1.0 for the alpha channel so the background is completely opaque
        return vec4(vec3(finalColor), 1.0);
    }";
}