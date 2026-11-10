Shader "PortalsFX2/MaskBlended"
{
	Properties
	{
		[HDR]_TintColor("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		[Space]
		[Space]
		[Space]
		[Toggle(USE_CUTOUT)] _UseCutout("Use Cutout", Int) = 0
		_MaskTex ("MaskTexture", 2D) = "white" {}
		_MaskCutout("MaskCutout", Range(0,1)) = .5
		[Space]
		[Space]
		[Space]
		_Thickness("Border Thickness", Range(0,.3)) = .05
		[HDR]_ThicknessColor("TBorder Color", Color) = (1,1,1,1)
		[Space]
		[Space]
		[Space]
		_AlphaPow("Alpha Pow", Float) = 1
		_AlphaMul("Alpha Mul", Float) = 1
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
			#pragma shader_feature_local USE_CUTOUT
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_MaskTex);
			SAMPLER(sampler_MaskTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _MaskTex_ST;
				float4 _TintColor;
				float4 _ThicknessColor;
				float _MaskCutout;
				float _Thickness;
				float _AlphaPow;
				float _AlphaMul;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv;
				output.color = input.color;
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				float mask = 1;
				float3 border = 0;

				#ifdef USE_CUTOUT
				float cutout = 1 - _MaskCutout;
				float maskCol = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, TRANSFORM_TEX(input.uv, _MaskTex)).r;
				border = (maskCol < saturate(cutout + _Thickness) && maskCol > cutout) ? _ThicknessColor.rgb : 0;
				mask = maskCol > cutout ? 1 : 0;
				#endif

				half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(input.uv, _MainTex)) * _TintColor * input.color;
				color.rgb += border;
				color.a = saturate(pow(max(color.a, 0.0001), _AlphaPow) * _AlphaMul * mask);
				return color;
			}
			ENDHLSL
		}
	}
	FallBack Off
}
