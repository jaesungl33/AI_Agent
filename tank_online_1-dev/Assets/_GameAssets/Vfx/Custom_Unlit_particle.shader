Shader "Custom/Custom_Unlit_particle"
{
    Properties
    {
        _MainTex("Particle Texture (Spritesheet)", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "gray" {}
        _MaskStrength("Mask Strength", Range(0,1)) = 1
        _MaskFollowMain("Mask Follow MainTex (0-1)", Range(0,1)) = 0
        _UseGrayAsAlpha("Use Grayscale As Alpha", Float) = 0
        _MainColor("Custom Tint Color", Color) = (1,1,1,1)

        _MainTex_ST("MainTex UV Tiling & Offset", Vector) = (1,1,0,0)
        _MaskTex_ST("MaskTex UV Tiling & Offset", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uvMain : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                float4 color : COLOR;
            };

            // --- Texture samplers (URP style)
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            float4 _MainColor;
            float _MaskStrength;
            float _MaskFollowMain;
            float _UseGrayAsAlpha;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uvMask = TRANSFORM_TEX(IN.uv, _MaskTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain);
                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uvMask);

                // Use grayscale as alpha (if enabled)
                if (_UseGrayAsAlpha > 0.5)
                {
                    float gray = dot(maskTex.rgb, float3(0.3, 0.59, 0.11));
                    maskTex.a = gray;
                }

                // Mix mask
                float maskValue = lerp(maskTex.a, maskTex.r, _MaskFollowMain);
                mainTex.a *= lerp(1.0, maskValue, _MaskStrength);

                // Apply tint and vertex color
                half4 col = mainTex * _MainColor * IN.color;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
