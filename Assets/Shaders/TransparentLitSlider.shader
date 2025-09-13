Shader "URP/TransparentLitSlider"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor]   _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Metallic   ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0,2)) = 1

        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)

        _Transparency ("Transparency (0=visible, 1=invisible)", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _Metallic;
                float  _Smoothness;
                float  _BumpScale;
                float4 _EmissionColor;
                float  _Transparency;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 tangentWS  : TEXCOORD2;
                float3 bitangentWS: TEXCOORD3;
                float2 uv         : TEXCOORD4;
                float4 shadowCoord: TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);

                VertexNormalInputs nI = GetVertexNormalInputs(v.normalOS, v.tangentOS);
                o.normalWS    = nI.normalWS;
                o.tangentWS   = nI.tangentWS;
                o.bitangentWS = nI.bitangentWS;

                o.uv = v.uv;

                #if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
                #else
                    o.shadowCoord = float4(0,0,0,0);
                #endif

                return o;
            }

            float3 ApplyNormalMap(float2 uv, float3 nWS, float3 tWS, float3 bWS)
            {
                float4 nTex = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv);
                float3 nTS  = UnpackNormalScale(nTex, _BumpScale);
                float3x3 TBN = float3x3(normalize(tWS), normalize(bWS), normalize(nWS));
                return normalize(mul(nTS, TBN));
            }

            float3 FresnelSchlick(float cosTheta, float3 F0)
            {
                return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
            }

            float DistributionGGX(float NdotH, float roughness)
            {
                float a  = roughness * roughness;
                float a2 = a * a;
                float d  = (NdotH * NdotH) * (a2 - 1.0) + 1.0;
                return a2 / (PI * d * d + 1e-5);
            }

            float GeometrySchlickGGX(float NdotV, float roughness)
            {
                float r  = roughness + 1.0;
                float k  = (r * r) / 8.0;
                return NdotV / (NdotV * (1.0 - k) + k + 1e-5);
            }

            float GeometrySmith(float NdotV, float NdotL, float roughness)
            {
                return GeometrySchlickGGX(NdotV, roughness) * GeometrySchlickGGX(NdotL, roughness);
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                float3 albedo = (baseSample.rgb * _BaseColor.rgb);

                float3 N = ApplyNormalMap(i.uv, i.normalWS, i.tangentWS, i.bitangentWS);

                float3 V = normalize(GetWorldSpaceViewDir(i.positionWS));
                float NdotV = saturate(dot(N, V));

                float metallic  = saturate(_Metallic);
                float roughness = saturate(1.0 - _Smoothness);

                float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo, metallic);

                float3 ambient = SampleSH(N) * albedo;

                Light mainLight = GetMainLight(i.shadowCoord);
                float3 L = normalize(-mainLight.direction);
                float3 H = normalize(L + V);
                float NdotL = saturate(dot(N, L));
                float NdotH = saturate(dot(N, H));

                float  D = DistributionGGX(NdotH, roughness);
                float  G = GeometrySmith(NdotV, NdotL, roughness);
                float3 F = FresnelSchlick(saturate(dot(H, V)), F0);

                float3 kS = F;
                float3 kD = (1.0 - kS) * (1.0 - metallic);

                float3 numerator   = D * G * F;
                float  denom       = 4.0 * max(NdotV, 1e-4) * max(NdotL, 1e-4) + 1e-5;
                float3 specular    = numerator / denom;

                float3 directMain  = (kD * albedo / PI + specular) * mainLight.color * (NdotL * mainLight.shadowAttenuation);

                float3 directAdd = 0;
                #if defined(_ADDITIONAL_LIGHTS)
                uint addCount = GetAdditionalLightsCount();
                for (uint li = 0; li < addCount; li++)
                {
                    Light aL = GetAdditionalLight(li, i.positionWS);
                    float3 aLdir = normalize(-aL.direction);
                    float  aNdotL = saturate(dot(N, aLdir));
                    float3 aH = normalize(aLdir + V);
                    float  aNdotH = saturate(dot(N, aH));
                    float3 aF = FresnelSchlick(saturate(dot(aH, V)), F0);
                    float3 a_kS = aF;
                    float3 a_kD = (1.0 - a_kS) * (1.0 - metallic);

                    float aD = DistributionGGX(aNdotH, roughness);
                    float aG = GeometrySmith(NdotV, aNdotL, roughness);
                    float3 aSpec = (aD * aG * aF) / (4.0 * max(NdotV, 1e-4) * max(aNdotL, 1e-4) + 1e-5);

                    directAdd += (a_kD * albedo / PI + aSpec) * aL.color * (aNdotL * aL.distanceAttenuation * aL.shadowAtten);
                }
                #endif

                // Emission
                float3 emission = _EmissionColor.rgb;

                float3 color = ambient + directMain + directAdd + emission;

                float alpha = (1.0 - _Transparency) * _BaseColor.a * baseSample.a;

                // Fog
                #if defined(FOG_ANY)
                    color = MixFog(color, i.positionCS.z);
                #endif

                return float4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
