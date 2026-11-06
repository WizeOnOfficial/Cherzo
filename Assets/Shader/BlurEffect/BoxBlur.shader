Shader "Custom/BoxBlur"
{
    Properties
    {
        _Blur ("Blur Strength (size of filter (2n+1)^2)", Integer) = 1
        _Scale ("Scale (texel offset)", Range(1, 5)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                // float2 uv : TEXCOORD0; -- UNNEEDED, NOT USING ANY TYPE OF TEXTURE
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                int _Blur;
                float _Scale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 OUT = 0.0;
                half2 pos = IN.screenPos.xy / IN.screenPos.w;
                half2 texel = _Scale * (1.0 / _ScreenParams.xy);
                int blur_Size = _Blur > 0 ? _Blur : 1;
                for(int i = -blur_Size; i <= blur_Size; i++) {
                    for(int j = -blur_Size; j <= blur_Size; j++) {
                        OUT += SAMPLE_TEXTURE2D(
                            _CameraOpaqueTexture,
                            sampler_CameraOpaqueTexture,
                            pos + (half2(i, j) * texel));
                    }
                }
                OUT = OUT / ((2 * blur_Size + 1) * (2 * blur_Size + 1));

                return half4(OUT.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}
