// KerbVisionIR - Grain/Noise Shader
// Film grain effect for night vision

Shader "Hidden/KerbVision/Grain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _Intensity ("Grain Intensity", Range(0, 1)) = 0.5
        _NoiseScale ("Noise Scale", Float) = 1.0
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
            sampler2D _NoiseTex;
            float _Intensity;
            float _NoiseScale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Simple pseudo-random function
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233)) + _Time.y) * 43758.5453);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Generate procedural noise (fast and simple)
                float noise = random(i.uv * _NoiseScale);
                
                // Remap noise from [0,1] to [-0.5, 0.5]
                noise = (noise - 0.5) * _Intensity;
                
                // Apply grain
                col.rgb += noise;
                
                return col;
            }
            ENDCG
        }
    }
}
