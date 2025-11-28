Shader "Custom/Shader_Sprite_custom_particle"
{
    Properties
    {
        _MainTex ("Particle Texture (SpriteSheet)", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "gray" {}       
        _MaskStrength ("Mask Strength", Range(0,1)) = 1  

        _MainColor ("MainTex Color", Color) = (1,1,1,1)  
        _MaskColor ("MaskTex Color", Color) = (1,1,1,1)  

        _MaskTiling ("MaskTex UV Tiling", Vector) = (1,1,0,0)
        _MaskOffset ("MaskTex UV Offset", Vector) = (0,0,0,0)

        _MaskFollowMain ("Mask Follow MainTex (0-1)", Range(0,1)) = 1

        [Enum(AlphaBlend,0,Additive,1)] _BlendMode ("Blend Mode", Float) = 0
        [Enum(Repeat,0,Clamp,1)] _MaskWrapMode ("MaskTex Wrap Mode", Float) = 0
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

            float4 _MaskTiling;
            float4 _MaskOffset;

            float _BlendMode;
            float _MaskStrength;
            float _MaskWrapMode;
            float _MaskFollowMain;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0; // SpriteSheet UV
                float4 color  : COLOR;     // ParticleSystem Color over Lifetime
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float2 uvMain    : TEXCOORD0;
                float2 uvMask    : TEXCOORD1;
                fixed4 color     : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // SpriteSheet UV
                o.uvMain = v.uv;

                // MaskTex UV
                float2 maskUV = v.uv * _MaskTiling.xy + _MaskOffset.xy;
                maskUV = lerp(maskUV, v.uv, _MaskFollowMain);

                if (_MaskWrapMode > 0.5)
                    maskUV = saturate(maskUV);

                o.uvMask = maskUV;

                // Particle Color (Start Color, Color over Lifetime, Alpha over Lifetime)
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // MainTex (SpriteSheet) * MainColor * Particle Color
                fixed4 mainTex = tex2D(_MainTex, i.uvMain) * _MainColor * i.color;

                // MaskTex
                fixed4 maskTex = tex2D(_MaskTex, i.uvMask) * _MaskColor;
                float maskLuminance = dot(maskTex.rgb, float3(0.299, 0.587, 0.114));

                float blendFactor = maskLuminance * _MaskStrength;

                fixed4 col;
                if (_BlendMode < 0.5)
                {
                    // AlphaBlend: blend RGB theo mask, alpha từ mainTex
                    col.rgb = lerp(mainTex.rgb, maskTex.rgb, blendFactor);
                    col.a   = mainTex.a;
                }
                else
                {
                    // Additive
                    col.rgb = mainTex.rgb + maskTex.rgb * blendFactor;
                    col.a   = mainTex.a;
                }

                return col;
            }
            ENDCG
        }
    }
}
