// KerbVisionIR - Color Grading Shader
// Saturation, Contrast, Brightness control

Shader "Hidden/KerbVision/ColorGrading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturation", Range(-1, 1)) = 0
        _Contrast ("Contrast", Range(-1, 1)) = 0
        _Brightness ("Brightness", Range(0, 2)) = 1
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
            float _Saturation;
            float _Contrast;
            float _Brightness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Convert RGB to grayscale
            float Luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply brightness
                col.rgb *= _Brightness;
                
                // Apply contrast
                float contrastFactor = 1.0 + _Contrast;
                col.rgb = (col.rgb - 0.5) * contrastFactor + 0.5;
                
                // Apply saturation
                float gray = Luminance(col.rgb);
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, 1.0 + _Saturation);
                
                return col;
            }
            ENDCG
        }
    }
}
