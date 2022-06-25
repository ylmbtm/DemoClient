// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Screen/Diffuse_Full" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"RenderType"="Opaque" }
		ZWrite Off
		ZTest Always
		pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half4 _Color;

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD;
		};
		
		struct base_date{
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD;
		};
		
		v2f vert(base_date v)
		{
			v2f o;
			
			float4 pos = UnityObjectToClipPos(v.vertex);
			o.pos = pos;
			float scale = 1.0;
			#if UNITY_UV_STARTS_AT_TOP
			scale = -1.0;
			#endif
			o.uv = (float2(pos.x / pos.w , pos.y * scale / pos.w) + 1) * 0.5;

			if(scale < 0)
				o.uv.y = 1 - o.uv.y;


			return o;
		}
		
		half4 frag(v2f i) : COLOR
		{
			return tex2D(_MainTex,i.uv) * _Color;
		}
		ENDCG
		}
	} 
}
