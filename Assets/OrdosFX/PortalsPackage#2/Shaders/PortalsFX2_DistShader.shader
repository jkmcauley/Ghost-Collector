Shader "PortalsFX2/DistortionCutout"
{
	Properties
	{
		[HDR]_TintColor("TintColor", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle(USE_ALPHA_CUTOUT)] _UseAlphaCutout ("Use Alpha Cutout from PS", int) = 0
		_CutoutTex("Cutout Texture", 2D) = "white" {}
		_DistTex("Distortion Texture", 2D) = "white" {}
		_DistStrength("Distortion Strength", Vector) = (1,1,1,1)
		_SoftFade("Soft Fade", Float) = 3
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off

		Pass
		{
			Name "UniversalForward"
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature_local USE_ALPHA_CUTOUT
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_CutoutTex);
			SAMPLER(sampler_CutoutTex);
			TEXTURE2D(_DistTex);
			SAMPLER(sampler_DistTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _CutoutTex_ST;
				float4 _DistTex_ST;
				float4 _TintColor;
				float4 _DistStrength;
				float _SoftFade;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS : POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 cutoutUV : TEXCOORD1;
				float2 distortUV : TEXCOORD2;
				float2 rotation : TEXCOORD3;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = TRANSFORM_TEX(input.uv.xy, _MainTex);
				output.cutoutUV = TRANSFORM_TEX(input.uv.xy, _CutoutTex);
				output.distortUV = TRANSFORM_TEX(input.uv.xy, _DistTex);
				output.rotation = input.uv.zw;
				output.color = input.color;
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				float2 distort = SAMPLE_TEXTURE2D(_DistTex, sampler_DistTex, input.distortUV + _Time.x * _DistStrength.zw).rg * _DistStrength.xy;
				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + input.rotation + distort);
				color = 2 * color * _TintColor * input.color;

				#ifdef USE_ALPHA_CUTOUT
				half cut = SAMPLE_TEXTURE2D(_CutoutTex, sampler_CutoutTex, input.cutoutUV - input.rotation).r;
				color.a = saturate(color.a * 4) * step(cut - input.color.a, color.a);
				#endif

				color.a = saturate(color.a);
				return color;
			}
			ENDHLSL
		}
	}
	FallBack Off
}
