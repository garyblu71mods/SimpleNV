// KerbVisionIR - Master Uber Shader
// Combined effects: Vignette + Color Grading + Grain

Shader "Hidden/KerbVision/Uber"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Vignette
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _VignetteSmoothness ("Vignette Smoothness", Range(0.01, 1)) = 0.3
        
        // Color Grading
        _Saturation ("Saturation", Range(-1, 1)) = 0
        _Contrast ("Contrast", Range(-1, 1)) = 0
        _Brightness ("Brightness", Range(0, 2)) = 1
        
        // Grain
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.5
        _GrainSize ("Grain Size", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ GRAIN_ENABLED
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            // Vignette
            float _VignetteIntensity;
            float _VignetteSmoothness;
            
            // Color Grading
            float _Saturation;
            float _Contrast;
            float _Brightness;
            
            // Grain
            float _GrainIntensity;
            float _GrainSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                    o.uv.y = 1 - o.uv.y;
                #endif
                
                return o;
            }
            
            // Convert RGB to grayscale
            float Luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }
            
            // Pseudo-random function
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)) + _Time.y) * 43758.5453123);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample input texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // === 1. COLOR GRADING ===
                
                // Brightness
                col.rgb *= _Brightness;
                
                // Contrast
                if (abs(_Contrast) > 0.01)
                {
                    float contrastFactor = 1.0 + _Contrast;
                    col.rgb = (col.rgb - 0.5) * contrastFactor + 0.5;
                }
                
                // Saturation
                if (abs(_Saturation) > 0.01)
                {
                    float gray = Luminance(col.rgb);
                    col.rgb = lerp(float3(gray, gray, gray), col.rgb, 1.0 + _Saturation);
                }
                
                // === 2. VIGNETTE ===
                
                if (_VignetteIntensity > 0.01)
                {
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(i.uv, center);
                    float vignette = smoothstep(0.5, 0.5 - _VignetteSmoothness, dist);
                    vignette = lerp(1.0, vignette, _VignetteIntensity);
                    col.rgb *= vignette;
                }
                
                // === 3. GRAIN ===
                
                #ifdef GRAIN_ENABLED
                if (_GrainIntensity > 0.01)
                {
                    // Generate noise
                    float noise = random(i.uv * _GrainSize * 100.0);
                    noise = (noise - 0.5) * _GrainIntensity * 0.1;
                    
                    // Apply grain
                    col.rgb += noise;
                }
                #endif
                
                // Clamp to valid range
                col.rgb = saturate(col.rgb);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback Off
}
