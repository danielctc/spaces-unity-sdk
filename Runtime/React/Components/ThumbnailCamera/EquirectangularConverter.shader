// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Credit: https://github.com/Mapiarz/CubemapToEquirectangular/blob/master/Assets/Shaders/CubemapToEquirectangular.shader

Shader "Hidden/I360CubemapToEquirectangular"
{
	Properties
	{
		_MainTex ("Cubemap (RGB)", CUBE) = "" {}
		_PaddingX ("Padding X", Float) = 0.0
	}

	Subshader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }
		
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#define PI    3.141592653589793
			#define TWOPI 6.283185307179587

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

			TEXTURECUBE(_MainTex);
			SAMPLER(sampler_MainTex);
			float _PaddingX;

			Varyings vert(Attributes input)
			{
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = (input.uv.xy + float2(_PaddingX,0)) * float2(TWOPI, PI);
				return output;
			}

			float4 frag(Varyings input) : SV_Target
			{
				float theta = input.uv.y;
				float phi = input.uv.x;
				float3 unit = float3(0,0,0);

				unit.x = sin(phi) * sin(theta) * -1;
				unit.y = cos(theta) * -1;
				unit.z = cos(phi) * sin(theta) * -1;

				return SAMPLE_TEXTURECUBE(_MainTex, sampler_MainTex, unit);
			}
			ENDHLSL
		}
	}
	Fallback Off
}