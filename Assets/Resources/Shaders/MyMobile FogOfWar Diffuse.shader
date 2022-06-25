Shader "MyMobile/FogOfWar/Diffuse" {
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FogTex("FogTex", 2D) = "white" {}
		_PassTex("PassTex",2D) = "black"{}
		_Pass("Pass",vector) = (0,0,0,0)
		_AlphaCon("AlphaCon",Range(0,1)) = 0.9
	}
	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Fog { Mode off }
					
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _FogTex;
			sampler2D _PassTex;
			sampler2D _CameraDepthTexture;

			half4 _Pass;

			uniform float4x4 _Inverse;
			uniform float4 _Params;
			uniform float4 _Pos;

			half _AlphaCon;

			struct Input
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			float3 CamToWorld (in float2 uv, in float depth)
			{
				float4 pos = float4(uv.x, uv.y, depth, 1.0);
				pos.xyz = pos.xyz * 2.0 - 1.0;
				pos = mul(_Inverse, pos);
				return pos.xyz / pos.w;
			}

			half4 frag (Input i) : COLOR
			{
				half4 original = tex2D(_MainTex, i.uv);

			#if SHADER_API_D3D9
				float2 depthUV = i.uv;
				depthUV.y = lerp(depthUV.y, 1.0 - depthUV.y, _Pos.w);
				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, depthUV));
				float3 pos = CamToWorld(depthUV, depth);
			#else
				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
				float3 pos = CamToWorld(i.uv, depth);
			#endif
	
				if (pos.y < 0.0)
				{
					float3 dir = normalize(pos - _Pos.xyz);
					pos = _Pos.xyz - dir * (_Pos.y / dir.y);
				}
	
				float2 uv = pos.xz * _Params.z + _Params.xy;
				half4 fogTex = tex2D(_FogTex,uv * 8);
				half4 passTex = tex2D(_PassTex,uv) * _Pass;
				half alpha = saturate(passTex.r + passTex.g + passTex.b + passTex.a) * _AlphaCon;
				half4 destColor;
				destColor.rgb = original.rgb + (fogTex.rgb - original.rgb) * alpha;
				destColor.a = original.a;

				return destColor;
			}
			ENDCG
		}
	}
}
