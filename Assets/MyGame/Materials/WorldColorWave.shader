Shader "Game/WorldColorWave"
{
    Properties
    {
        _BaseMap ("Original Texture", 2D) = "white" {}
        _BaseColor ("Original Color", Color) = (1,1,1,1)
        _GrayColor ("Gray World", Color) = (0.65,0.65,0.65,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _GrayColor;
            CBUFFER_END
            float4 _WorldWaveOrigin;
            float _WorldWaveRadius;
            float _WorldWaveSoftness;
            float4 _WorldWaveColor;
            float _WorldWaveGlow;
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float distanceToTower = distance(input.positionWS.xz, _WorldWaveOrigin.xz);
                float edge = (distanceToTower - _WorldWaveRadius) / max(_WorldWaveSoftness, 0.01);
                half reveal = 1.0 - smoothstep(-1.0, 1.0, edge);
                half3 original = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 albedo = lerp(_GrayColor.rgb, original, reveal);
                half3 normal = normalize(input.normalWS);
                Light sun = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 lighting = max(SampleSH(normal), half3(0.22, 0.22, 0.22)) +
                    sun.color * saturate(dot(normal, sun.direction)) * sun.shadowAttenuation;
                half glow = exp2(-edge * edge * 3.0) * _WorldWaveGlow;
                half3 warmLight = lerp(_WorldWaveColor.rgb, half3(1.0, 0.9, 0.65), 0.3);
                return half4(albedo * lighting + warmLight * glow * 0.65, 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
