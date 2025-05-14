Shader "Custom/BillboardPortal"
{
    Properties
    {
        _MainTex ("Portal Texture", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        [HDR] _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowPower ("Glow Power", Range(0.1, 10.0)) = 2.0
        _GlowScale ("Glow Scale", Range(1.0, 2.0)) = 1.2
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 100

        // First pass - render the glow
        Pass
        {
            Name "Glow"
            ZWrite Off
            ZTest LEqual
            Blend One One
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _GlowColor;
                float _GlowPower;
                float _GlowScale;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Billboard calculation
                float3 worldPos = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float3 right = normalize(cross(float3(0, 1, 0), viewDir));
                float3 up = cross(viewDir, right);
                
                // Scale the mesh
                float3 scaledPos = input.positionOS.xyz * _GlowScale;
                
                // Apply billboard transformation
                float3 billboardPos = worldPos 
                    + right * scaledPos.x 
                    + up * scaledPos.y;
                
                output.positionCS = TransformWorldToHClip(billboardPos);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Simple edge glow
                float dist = length(input.uv - float2(0.5, 0.5));
                float glow = 1.0 - smoothstep(0.4, 0.5, dist);
                glow = pow(glow, _GlowPower);
                
                return _GlowColor * glow;
            }
            ENDHLSL
        }

        // Second pass - render the main portal
        Pass
        {
            Name "Portal"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionColor;
                float _DistortionStrength;
                float _DistortionSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Billboard calculation
                float3 worldPos = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float3 right = normalize(cross(float3(0, 1, 0), viewDir));
                float3 up = cross(viewDir, right);
                
                // Apply billboard transformation
                float3 billboardPos = worldPos 
                    + right * input.positionOS.x 
                    + up * input.positionOS.y;
                
                output.positionCS = TransformWorldToHClip(billboardPos);
                // Flip UV coordinates horizontally
                float2 uv = input.uv;
                uv.x = 1.0 - uv.x;
                output.uv = TRANSFORM_TEX(uv, _MainTex);
                output.worldPos = billboardPos;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Calculate distance from center for distortion falloff
                float2 center = float2(0.5, 0.5);
                float dist = length(input.uv - center);
                
                // Calculate lens distortion
                float2 distortion = float2(
                    sin(_Time.y * _DistortionSpeed + input.worldPos.x) * _DistortionStrength,
                    cos(_Time.y * _DistortionSpeed + input.worldPos.y) * _DistortionStrength
                );
                
                // Apply distortion to UVs with falloff
                float2 distortedUV = input.uv + distortion * (1.0 - dist);
                
                // Sample texture with distorted UVs
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                col.rgb += _EmissionColor.rgb;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
} 