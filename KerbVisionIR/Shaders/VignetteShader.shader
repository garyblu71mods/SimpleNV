// KerbVisionIR - Vignette Shader
// Simple vignette effect for night vision

Shader "Hidden/KerbVision/Vignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _Smoothness ("Vignette Smoothness", Range(0.01, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
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
            float _Intensity;
            float _Smoothness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate distance from center
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // Vignette calculation
                float vignette = smoothstep(0.5, 0.5 - _Smoothness, dist);
                vignette = lerp(1.0, vignette, _Intensity);
                
                // Apply vignette
                col.rgb *= vignette;
                
                return col;
            }
            ENDCG
        }
    }
}
