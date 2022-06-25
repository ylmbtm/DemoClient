// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Screen/Full" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Subtract("Subtract",Range(-1,1)) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent+100" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest

		sampler2D _MainTex;
		half _Subtract;
		
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
			return o;
		}
		
		half4 frag(v2f i) : COLOR
		{
			half4 outColor = tex2D(_MainTex,i.uv);
			outColor.a -= _Subtract;
			return outColor;
		}
		ENDCG
		}
	} 
}
