// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/UI/Scall" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ScallTex("ScallTex",2D) = "black"{}
		_ScallColor("ScallColor",Color) = (0.5,0.5,0.5,0.5)
		_Percent("Percent",Range(0,10)) = 1
		_GrowVal("GrowVal",Range(0.1,1)) = 0.5
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		
		pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest

		sampler2D _MainTex;
		sampler2D _ScallTex;
		float4 _ScallColor;
		float _Percent;
		float _GrowVal;
		
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD;
		};
		
		struct appdate_test
		{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD;
		};
		
		v2f vert(appdate_test v)
		{
			v2f o;
			float pos = v.vertex;
			
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord.xy;
			
			return o;
		}
		
		half4 frag(v2f i) : COLOR
		{
			float uvoffset = (1 - _Percent) * 0.5;
			float2 uv = i.uv * _Percent + uvoffset;
			
			half4 mainTex = tex2D(_MainTex,i.uv);
			half4 scallTex = tex2D(_ScallTex,uv) * _ScallColor;
			
			half alpha = saturate((10 - _Percent) * _GrowVal);
			
			half4 outColor;
			outColor.rgb = mainTex.rgb + scallTex.rgb * alpha;
			outColor.a = mainTex.a;
			return outColor;
		}
		ENDCG
		}
	} 
}
