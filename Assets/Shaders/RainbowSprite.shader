Shader "Custom/SoftRainbowUI"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Speed ("Color Change Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Speed;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 SoftRainbowColor(float t)
            {
                // Normalize time to [0, 1] range
                float3 color;
                float phase = t * 0.1; // Adjust the phase speed
                color = float3(
                    0.5 + 0.5 * sin(phase + 0.0) * 0.7,
                    0.5 + 0.5 * sin(phase + 2.0) * 0.7,
                    0.5 + 0.5 * sin(phase + 4.0) * 0.7
                );
                return color;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Calculate time-based color transition
                float t = _Time.y * _Speed;
                float3 softRainbowColor = SoftRainbowColor(t);
                
                // Sample the texture color
                half4 texColor = tex2D(_MainTex, i.uv);
                
                // Blend the soft rainbow color with the texture color
                return half4(softRainbowColor * texColor.rgb, texColor.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
