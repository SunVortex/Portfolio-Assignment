Shader "PostProcessing/AnimeFullScreenShader"
{
    Properties
    {
        _ShadowColor ("Anime Shadow Tint", Color) = (0.5, 0.45, 0.55, 1.0)
        _PosterizeLevels ("Color Bands", Range(2, 16)) = 4
        _Saturation ("Vibrancy Boost", Range(0.5, 2.0)) = 1.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off 
        ZTest Always
        Cull Off

        Pass
        {
            Name "AnimePostPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Native URP Blit Texture Reference
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float4 _ShadowColor;
            float _PosterizeLevels;
            float _Saturation;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // Sample camera screen color from BlitTexture
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, input.uv);

                // Posterize brightness levels for anime toon stepping
                float3 quantized = floor(col.rgb * _PosterizeLevels) / _PosterizeLevels;

                // Tint darker areas with anime shadow tone
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                if (luminance < 0.35)
                {
                    quantized *= _ShadowColor.rgb;
                }

                // Apply color saturation boost
                float3 grayscale = float3(luminance, luminance, luminance);
                float3 finalColor = lerp(grayscale, quantized, _Saturation);

                return float4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
}