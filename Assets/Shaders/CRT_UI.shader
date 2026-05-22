Shader "Custom/CRT_UI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distortion ("Distortion", Float) = 0.15
        _Scanline ("Scanline Intensity", Float) = 0.5
        _RGBNoise ("RGB Split & Noise", Float) = 0.003
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background"}
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
            float _Distortion;
            float _Scanline;
            float _RGBNoise;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float rand(float2 co) {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Искажение (Barrel Distortion)
                float2 uv = i.uv - 0.5;
                float rsq = uv.x*uv.x + uv.y*uv.y;
                uv = uv + uv * (_Distortion * rsq);
                uv += 0.5;

                // Черные края, если текстура вылезает за пределы
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return fixed4(0,0,0,1);

                // 2. Хроматическая аберрация (RGB Split) + Шум
                float noise = (rand(uv + _Time.y) - 0.5) * 2.0; // от -1 до 1
                float offset = _RGBNoise * (1.0 + noise * 0.5);
                
                fixed r = tex2D(_MainTex, uv + float2(offset, 0)).r;
                fixed g = tex2D(_MainTex, uv).g;
                fixed b = tex2D(_MainTex, uv - float2(offset, 0)).b;

                fixed4 col = fixed4(r, g, b, 1.0);

                // 3. Сканлайны (Scanlines)
                // Делаем полосы зависящие от Y координаты
                float scan = sin(uv.y * 800.0) * 0.04 * _Scanline;
                col.rgb -= scan;

                // 4. Виньетка (затемнение по краям)
                float vignette = length(i.uv - 0.5);
                vignette = smoothstep(0.8, 0.4, vignette);
                col.rgb *= vignette;

                // 5. Легкий зеленый оттенок (фосфор)
                col.g *= 1.1; 
                col.r *= 0.9;
                col.b *= 0.9;

                return col;
            }
            ENDCG
        }
    }
}
