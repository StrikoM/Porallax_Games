Shader "UI/CRT_URP_Safe"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _VignettePower ("Vignette Power", Range(0, 5)) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _ScanlineIntensity;
            float _VignettePower;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Изначально всё прозрачное
                fixed4 col = fixed4(0, 0, 0, 0);

                // 1. Scanlines (Темные полоски, всего 5% силы)
                float scan = sin(i.uv.y * 1500.0);
                float scanline = clamp(scan, 0.0, 1.0) * 0.05;

                // 2. Vignette (Затемнение углов)
                float2 uv = i.uv * 2.0 - 1.0;
                float dist = dot(uv, uv);
                // Плавный переход от центра (чисто) к краям (темно)
                float vignette = smoothstep(0.4, 1.5, dist * (2.0 / _VignettePower));

                // Итоговый цвет: ЧЕРНЫЙ, меняем только прозрачность
                col.rgb = half3(0, 0, 0);
                // Прозрачность складывается из полосок и виньетки
                col.a = scanline + (vignette * 0.7);

                // 3. Зернистость (Noise) - очень слабая
                float noise = (frac(sin(dot(i.uv, float2(12.9898,78.233)*_Time.y)) * 43758.5453));
                col.a += noise * 0.02;

                return col;
            }
            ENDCG
        }
    }
}
