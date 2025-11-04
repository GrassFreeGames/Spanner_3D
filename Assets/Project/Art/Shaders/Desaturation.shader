Shader "Hidden/Desaturation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturation", Range(0, 1)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Saturation;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Convert to grayscale using luminance formula
                // This formula accounts for human eye sensitivity to different colors
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                
                // Lerp between grayscale and original based on saturation
                // _Saturation = 1.0 -> full color
                // _Saturation = 0.0 -> black & white
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, _Saturation);
                
                return col;
            }
            ENDCG
        }
    }
}
