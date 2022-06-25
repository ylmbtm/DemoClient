// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Cloud/GrayscaleToAlpha" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FadeOutDistNear ("Near fadeout dist", float) = 10	
		_FadeOutDistFar ("Far fadeout dist", float) = 10000
		_UvOffset ("UVOffset(XY-Texture1,ZW-Texture2)",Vector) = (1,1,1,1)
		_GrayscaleToAlpha("GrayscaleToAlpha",Range(0,1)) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		
		Blend SrcAlpha One
		ZWrite off
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _FadeOutDistNear;
			half _FadeOutDistFar;
			half4 _UvOffset;
			half _GrayscaleToAlpha;
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
			};
			
			struct base_date{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			v2f vert(base_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord0,_MainTex);
				
				half4 offset = _UvOffset * _Time.y;
				o.uv.xy += offset.xy;
				
				float4 viewPos = mul(UNITY_MATRIX_MV,v.vertex);
				float dist = -viewPos.z;
				float nfadeout = saturate(dist / _FadeOutDistNear);
				float ffadeout = 1 - saturate(max(dist - _FadeOutDistFar,0) * 0.2);
				
				ffadeout *= ffadeout;
				nfadeout *= nfadeout;
				nfadeout *= nfadeout;
				nfadeout *= ffadeout;
				
				o.uv.z = nfadeout;
				o.uv.z = 1 - o.uv.z;
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				half4 texColor = tex2D(_MainTex,i.uv.xy);
				if(_GrayscaleToAlpha)
				{
					texColor.a = (texColor.r + texColor.g + texColor.b) / 3;
				}
				texColor.a *= i.uv.z;
				return texColor;
			}
			ENDCG
		}
	}
}
