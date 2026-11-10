// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AlchemistLab/UI/Cardio" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Scroll("Scroll", Float) = 0
	}
	SubShader {
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ZWrite Off
            ZTest Off
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			sampler2D _MainTex;
			float _Scroll;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				#ifdef UNITY_HALF_TEXEL_OFFSET
					OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1,1);
				#endif
				OUT.color = IN.color;
				return OUT;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				float x = input.texcoord.x * 0.5 + _Scroll;
				x = frac(x);
				x = abs(x * 2.0 - 1.0);
				float4 wave = tex2D(_MainTex, float2(x, input.texcoord.y)) * input.color;
				float3 glass = float3(0.03, 0.12, 0.06);
				float3 rgb = lerp(glass, wave.rgb, wave.a);
				return float4(rgb, input.color.a);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
