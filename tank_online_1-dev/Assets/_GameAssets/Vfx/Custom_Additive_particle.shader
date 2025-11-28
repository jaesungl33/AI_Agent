Shader "Custom_Additive_particle"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "gray" {}
        _MaskStrength ("Mask Strength", Range(0,1)) = 1  

        [HDR] _MainColor ("MainTex Color HDR", Color) = (1,1,1,1)  
        [HDR] _MaskColor ("MaskTex Color HDR", Color) = (1,1,1,1)  

        _MainEmission ("MainTex Emission", Range(0,10)) = 1
        _MaskEmission ("MaskTex Emission", Range(0,10)) = 1

        _MainTiling ("MainTex UV Tiling", Vector) = (1,1,0,0)
        _MainOffset ("MainTex UV Offset", Vector) = (0,0,0,0)

        _MaskTiling ("MaskTex UV Tiling", Vector) = (1,1,0,0)
        _MaskOffset ("MaskTex UV Offset", Vector) = (0,0,0,0)

        _MaskFollowMain ("Mask Follow MainTex (0-1)", Range(0,1)) = 1

        // 🔹 Scroll MainTex
        [Toggle] _MainTexScrollToggle ("Enable MainTex Scroll", Float) = 0
        _MainTexScrollSpeed ("MainTex Scroll Speed (XY)", Vector) = (0,0,0,0)

        // 🔹 Scroll MaskTex
        [Toggle] _MaskTexScrollToggle ("Enable MaskTex Scroll", Float) = 0
        _MaskTexScrollSpeed ("MaskTex Scroll Speed (XY)", Vector) = (0,0,0,0)

        [Enum(AlphaBlend,0,Additive,1)] _BlendMode ("Blend Mode", Float) = 1
        [Enum(Repeat,0,Clamp,1)] _MainWrapMode ("MainTex Wrap Mode", Float) = 0
        [Enum(Repeat,0,Clamp,1)] _MaskWrapMode ("MaskTex Wrap Mode", Float) = 0

        // 🔹 Vertex Noise
        [Toggle] _NoiseToggle ("Enable Vertex Noise", Float) = 0
        _NoiseStrength ("Vertex Noise Strength", Range(0,1)) = 0.2
        _NoiseSpeed ("Noise Speed", Range(0,10)) = 1
        _NoiseScale ("Noise Scale", Range(0.1,10)) = 1

        // 🔹 Grayscale → Alpha
        [Toggle] _GrayAsAlpha ("Use Grayscale As Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            Name "ParticlePass"
            // 🔹 Additive mặc định
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;

            fixed4 _MainColor;
            fixed4 _MaskColor;
            float  _MainEmission;
            float  _MaskEmission;

            float4 _MainTiling;
            float4 _MainOffset;
            float4 _MaskTiling;
            float4 _MaskOffset;

            float _BlendMode;
            float _MaskStrength;
            float _MainWrapMode;
            float _MaskWrapMode;
            float _MaskFollowMain;

            float _MainTexScrollToggle;
            float4 _MainTexScrollSpeed;

            float _MaskTexScrollToggle;
            float4 _MaskTexScrollSpeed;

            float _NoiseToggle;
            float _NoiseStrength;
            float _NoiseSpeed;
            float _NoiseScale;

            float _GrayAsAlpha; 

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 custom   : TEXCOORD1;
                float4 custom2  : TEXCOORD2; 
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float2 uvMain    : TEXCOORD0;
                float2 uvMask    : TEXCOORD1;
                fixed4 color     : COLOR;
                float2 intensity : TEXCOORD2;
            };

            float noise(float3 pos, float t)
            {
                return sin(pos.x * 7.0 + t) * cos(pos.z * 5.0 + t * 0.5);
            }

            v2f vert (appdata v)
            {
                v2f o;

                if (_NoiseToggle > 0.5)
                {
                    float t = _Time.y * _NoiseSpeed;
                    float n = noise(v.vertex.xyz * _NoiseScale, t);
                    v.vertex.y += n * _NoiseStrength;
                }

                o.pos = UnityObjectToClipPos(v.vertex);

                float2 mainUV = v.uv * _MainTiling.xy + _MainOffset.xy;
                float2 customOffset = v.custom.xy;
                float2 customTiling = v.custom.zw;
                if (abs(customTiling.x) < 1e-6) customTiling.x = 1.0;
                if (abs(customTiling.y) < 1e-6) customTiling.y = 1.0;
                mainUV = mainUV * customTiling + customOffset;

                if (_MainTexScrollToggle > 0.5)
                {
                    float2 speedMulMain = (abs(v.custom.xy) < 1e-6) ? 1.0 : v.custom.xy;
                    mainUV += _MainTexScrollSpeed.xy * speedMulMain * _Time.y;
                }

                if (_MainWrapMode > 0.5)
                    mainUV = saturate(mainUV);

                o.uvMain = mainUV;

                float2 maskUV = v.uv * _MaskTiling.xy + _MaskOffset.xy;
                float2 mainUV_noWrap = v.uv * _MainTiling.xy + _MainOffset.xy;
                mainUV_noWrap = mainUV_noWrap * customTiling + customOffset;

                maskUV = lerp(maskUV, mainUV_noWrap, _MaskFollowMain);

                if (_MaskTexScrollToggle > 0.5)
                {
                    float2 speedMulMask = (abs(v.custom.zw) < 1e-6) ? 1.0 : v.custom.zw;
                    maskUV += _MaskTexScrollSpeed.xy * speedMulMask * _Time.y;
                }

                if (_MaskWrapMode > 0.5)
                    maskUV = saturate(maskUV);

                o.uvMask = maskUV;
                o.color = v.color;

                float mainI = (abs(v.custom2.x) < 1e-6) ? 1.0 : v.custom2.x;
                float maskI = (abs(v.custom2.y) < 1e-6) ? 1.0 : v.custom2.w;
                o.intensity = float2(mainI, maskI);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uvMain);
                fixed4 maskTex = tex2D(_MaskTex, i.uvMask);

                // 🔹 Grayscale as Alpha
                if (_GrayAsAlpha > 0.5)
                {
                    float gray = dot(mainTex.rgb, float3(0.333, 0.333, 0.333));
                    mainTex = fixed4(1,1,1, gray);
                }

                // 🔹 Nếu ảnh alpha-only (RGB = 0), copy alpha vào RGB
                if (all(mainTex.rgb == 0))
                {
                    mainTex.rgb = mainTex.a.xxx;
                }

                // Áp dụng màu và LineRenderer color
                mainTex.rgb *= _MainColor.rgb * i.color.rgb;
                maskTex.rgb *= _MaskColor.rgb * i.color.rgb;

                // Emission
                mainTex.rgb *= (_MainEmission * i.intensity.x);
                maskTex.rgb *= (_MaskEmission * i.intensity.y);

                float blendFactor = saturate(maskTex.a * _MaskStrength);

                fixed4 col;
                if (_BlendMode < 0.5) 
                {
                    col.rgb = lerp(mainTex.rgb, maskTex.rgb, blendFactor);
                }
                else 
                {
                    col.rgb = mainTex.rgb + maskTex.rgb * blendFactor;
                }

                // 🔹 Additive = RGB × Alpha tổng hợp
                col.rgb *= mainTex.a * _MainColor.a * i.color.a;
                col.a = 1;

                return col;
            }
            ENDCG
        }
    }
}
