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
            float _EffectType;

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

                // Convert color to grayscale for intensity calculation
                float grayscale = dot(col.rgb, float3(0.3, 0.59, 0.11));

                // Black background setup
                fixed4 background = fixed4(0.0, 0.0, 0.0, 1.0);

                // Effect 1: Heat Map (Red to Yellow Gradient)
                if (_EffectType == 1)
                {
                    float heatIntensity = pow(grayscale, 1.5); // Emphasize brightness
                    col.rgb = lerp(float3(0.2, 0.0, 0.0), float3(1.0, 0.0, 0.0), heatIntensity); // Dark red to bright red
                    col.rgb = lerp(col.rgb, float3(1.0, 1.0, 0.0), heatIntensity * 2.0); // Transition to yellow
                    col.rgb = lerp(background.rgb, col.rgb, grayscale);
                }
                // Effect 2: Cool Heat Map (Blue to Cyan Gradient)
                else if (_EffectType == 2)
                {
                    float coolIntensity = pow(grayscale, 1.5); // Emphasize brightness
                    col.rgb = lerp(float3(0.0, 0.0, 0.2), float3(0.0, 0.0, 1.0), coolIntensity); // Dark blue to bright blue
                    col.rgb = lerp(col.rgb, float3(0.0, 1.0, 1.0), coolIntensity * 2.0); // Transition to cyan
                    col.rgb = lerp(background.rgb, col.rgb, grayscale);
                }
                // Effect 3: Sunset Map (Purple to Orange Gradient)
                else if (_EffectType == 3)
                {
                    float sunsetIntensity = pow(grayscale, 1.5); // Emphasize brightness
                    col.rgb = lerp(float3(0.3, 0.0, 0.5), float3(1.0, 0.2, 0.5), sunsetIntensity); // Deep purple to magenta
                    col.rgb = lerp(col.rgb, float3(1.0, 0.5, 0.0), sunsetIntensity * 2.0); // Transition to orange
                    col.rgb = lerp(background.rgb, col.rgb, grayscale);
                }
                // Effect 4: Green Glow Map (Dark Green to Bright Lime)
                else if (_EffectType == 4)
                {
                    float glowIntensity = pow(grayscale, 1.5); // Emphasize brightness
                    col.rgb = lerp(float3(0.0, 0.2, 0.0), float3(0.0, 1.0, 0.0), glowIntensity); // Dark green to bright green
                    col.rgb = lerp(col.rgb, float3(0.5, 1.0, 0.2), glowIntensity * 2.0); // Transition to lime
                    col.rgb = lerp(background.rgb, col.rgb, grayscale);
                }

                // Ensure the output color stays within valid bounds
                col.rgb = saturate(col.rgb);

                return col;
            }
            ENDCG
        }
    }
}
