Shader "Custom/SpriteGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.8, 0.3, 1)
        _GlowRadius ("Glow Radius (texels)", Float) = 6
        _GlowIntensity ("Glow Intensity", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define RINGS 4
            #define DIRS 12

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                half4 _GlowColor;
                float _GlowRadius;
                float _GlowIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half SampleA (float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

                half glow = 0;
                [unroll] for (int r = 1; r <= RINGS; r++)
                {
                    float t = r / (float)RINGS;
                    float2 d = _MainTex_TexelSize.xy * _GlowRadius * t;
                    half w = 1 - t;
                    [unroll] for (int k = 0; k < DIRS; k++)
                    {
                        float ang = k * (6.2831853 / DIRS);
                        glow = max(glow, SampleA(i.uv + d * float2(cos(ang), sin(ang))) * w);
                    }
                }
                glow = saturate(glow * _GlowIntensity);
                glow *= glow;

                half glowA = glow * _GlowColor.a;
                half outA  = c.a + glowA * (1 - c.a);

                half4 col;
                col.rgb = (c.rgb * c.a + _GlowColor.rgb * glowA * (1 - c.a)) / max(outA, 1e-4);
                col.a   = outA;
                return col;
            }
            ENDHLSL
        }
    }
}