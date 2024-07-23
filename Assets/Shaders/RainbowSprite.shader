Shader "Custom/RainbowUI"
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

            float3 RainbowColor(float t)
            {
                float3 color;
                color = float3(0.5 + 0.5 * sin(t + 0.0), 0.5 + 0.5 * sin(t + 2.0), 0.5 + 0.5 * sin(t + 4.0));
                return color;
            }

            half4 frag (v2f i) : SV_Target
            {
                float t = _Time.y * _Speed;
                float3 rainbowColor = RainbowColor(t);
                
                half4 texColor = tex2D(_MainTex, i.uv);
                
                return half4(rainbowColor * texColor.rgb, texColor.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
