Shader "Custom/CameraEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EffectType ("Effect Type", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed _EffectType;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Define a tolerance for "close enough to white"
                const fixed epsilon = 0.2;

                // Check if the color is close to white
                if (col.r > 1.0 - epsilon && col.g > 1.0 - epsilon && col.b > 1.0 - epsilon)
                {
                    return col; // Skip effect processing for near-white colors
                }

                // Convert color to grayscale for intensity calculation
                fixed grayscale = dot(col.rgb, fixed3(0.3, 0.59, 0.11));

                // Night Vision Effects
                if (_EffectType == 1) // Grey Theme Night Mode
                {
                    // Apply a blue-gray tint for night vision
                    col.rgb = lerp(fixed3(0.0, 0.0, 0.2), fixed3(0.3, 0.3, 0.4), grayscale * 1.5); 
                }
                else if (_EffectType == 2) // Red Theme Night Mode
                {
                    // Apply a deep red tint for night vision
                    col.rgb = lerp(fixed3(0.2, 0.0, 0.0), fixed3(0.8, 0.1, 0.1), grayscale * 1.5);
                }
                else if (_EffectType == 3) // Green Theme Night Mode
                {
                    // Apply a green tint for night vision (classic night vision look)
                    col.rgb = lerp(fixed3(0.0, 0.2, 0.0), fixed3(0.0, 1.0, 0.0), grayscale * 1.5);
                }
                else if (_EffectType == 4) // Cyan Theme Night Mode
                {
                    // Apply a cyan tint for night vision
                    col.rgb = lerp(fixed3(0.0, 0.2, 0.2), fixed3(0.0, 0.8, 0.8), grayscale * 1.5);
                }

                // Clamp colors to ensure they remain within valid range
                col.rgb = saturate(col.rgb);

                return col;
            }
            ENDCG
        }
    }
}
