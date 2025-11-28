Shader "Custom/Shader_custom_particle"
{
    Properties
    {
        _MainTex ("Particle Texture (Spritesheet)", 2D) = "white" {}
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

        // 🔹 Blend Mode mở rộng
        [Enum(AlphaBlend,0,Additive,1,PremultipliedAlpha,2,Multiply,3,Screen,4,SoftAdditive,5)]
        _BlendMode ("Blend Mode", Float) = 0

        [Enum(Repeat,0,Clamp,1)] _MainWrapMode ("MainTex Wrap Mode", Float) = 0
        [Enum(Repeat,0,Clamp,1)] _MaskWrapMode ("MaskTex Wrap Mode", Float) = 0

        // 🔹 Vertex Noise
        [Toggle] _NoiseToggle ("Enable Vertex Noise", Float) = 0
        _NoiseStrength ("Vertex Noise Strength", Range(0,1)) = 0.2
        _NoiseSpeed ("Noise Speed", Range(0,10)) = 1
        _NoiseScale ("Noise Scale", Range(0.1,10)) = 1

        // 🔹 Grayscale → Alpha
        [Toggle] _GrayAsAlpha ("Use Grayscale As Alpha", Float) = 0

        // 🔹 Single Channel
        [Enum(Red,0,Green,1,Blue,2,Alpha,3,None,4)] _UseSingleChannel ("Use Single Channel", Float) = 4

        // 🔹 Flipbook Spritesheet
        [Toggle] _FlipbookEnable ("Enable Flipbook Animation", Float) = 0
        _FlipbookRows ("Flipbook Rows", Float) = 4
        _FlipbookCols ("Flipbook Columns", Float) = 4
        _FlipbookSpeed ("Flipbook FPS", Float) = 8
        [Enum(Loop,0,PingPong,1,Once,2)] _FlipbookMode ("Flipbook Mode", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ParticlePass"
            Blend SrcAlpha OneMinusSrcAlpha

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
            float _UseSingleChannel;

            // Flipbook
            float _FlipbookEnable;
            float _FlipbookRows;
            float _FlipbookCols;
            float _FlipbookSpeed;
            float _FlipbookMode;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 custom : TEXCOORD1;
                float4 custom2 : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                fixed4 color : COLOR;
                float2 intensity : TEXCOORD2;
            };

            float noise(float3 pos, float t)
            {
                return sin(pos.x * 7.0 + t) * cos(pos.z * 5.0 + t * 0.5);
            }

            v2f vert(appdata v)
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

                // 🔹 Scroll UV
                if (_MainTexScrollToggle > 0.5)
                {
                    float2 speedMulMain = (abs(v.custom.xy) < 1e-6) ? 1.0 : v.custom.xy;
                    mainUV += _MainTexScrollSpeed.xy * speedMulMain * _Time.y;
                }

                // 🔹 Flipbook logic
                if (_FlipbookEnable > 0.5)
                {
                    float totalFrames = _FlipbookRows * _FlipbookCols;
                    float frameTime = 1.0 / _FlipbookSpeed;
                    float time = _Time.y / frameTime;
                    float frame = floor(time);

                    // Mode: Loop, PingPong, Once
                    if (_FlipbookMode == 1) // PingPong
                        frame = abs(fmod(frame, totalFrames * 2) - totalFrames);
                    else if (_FlipbookMode == 2) // Once
                        frame = min(frame, totalFrames - 1);
                    else // Loop
                        frame = fmod(frame, totalFrames);

                    float row = floor(frame / _FlipbookCols);
                    float col = fmod(frame, _FlipbookCols);

                    float2 cellSize = 1.0 / float2(_FlipbookCols, _FlipbookRows);
                    mainUV = mainUV * cellSize + float2(col, (_FlipbookRows - 1 - row)) * cellSize;
                }

                if (_MainWrapMode > 0.5)
                    mainUV = saturate(mainUV);

                o.uvMain = mainUV;

                // Mask UV
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

            fixed4 ApplySingleChannel(fixed4 tex)
            {
                if (_UseSingleChannel == 0) return fixed4(tex.r, tex.r, tex.r, tex.r);
                if (_UseSingleChannel == 1) return fixed4(tex.g, tex.g, tex.g, tex.g);
                if (_UseSingleChannel == 2) return fixed4(tex.b, tex.b, tex.b, tex.b);
                if (_UseSingleChannel == 3) return fixed4(tex.a, tex.a, tex.a, tex.a);
                return tex;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uvMain);
                fixed4 maskTex = tex2D(_MaskTex, i.uvMask);

                mainTex = ApplySingleChannel(mainTex);

                if (_GrayAsAlpha > 0.5)
                {
                    float gray = dot(mainTex.rgb, float3(0.333,0.333,0.333));
                    mainTex = fixed4(1,1,1, gray);
                }

                mainTex.rgb *= _MainColor.rgb * i.color.rgb * (_MainEmission * i.intensity.x);
                maskTex.rgb *= _MaskColor.rgb * i.color.rgb * (_MaskEmission * i.intensity.y);

                float blendFactor = saturate(maskTex.a * _MaskStrength);

                fixed4 col = 0;

                if (_BlendMode == 0) col.rgb = lerp(mainTex.rgb, maskTex.rgb, blendFactor);
                else if (_BlendMode == 1) col.rgb = mainTex.rgb + maskTex.rgb * blendFactor;
                else if (_BlendMode == 2) col.rgb = lerp(mainTex.rgb, maskTex.rgb, blendFactor);
                else if (_BlendMode == 3) col.rgb = mainTex.rgb * (maskTex.rgb * blendFactor + (1 - blendFactor));
                else if (_BlendMode == 4) col.rgb = 1 - (1 - mainTex.rgb) * (1 - maskTex.rgb * blendFactor);
                else if (_BlendMode == 5) col.rgb = mainTex.rgb + maskTex.rgb * blendFactor * (1 - mainTex.a);

                col.a = mainTex.a * _MainColor.a * i.color.a;
                return col;
            }
            ENDCG
        }
    }
}
