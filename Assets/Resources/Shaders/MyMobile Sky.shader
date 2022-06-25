// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Sky" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	//_Month("Month",2D) = "black"{}
	_ScrollX ("Base layer Scroll speed X", Float) = 1.0
	_ScrollY ("Base layer Scroll speed Y", Float) = 0.0
	_Scroll2X ("2nd layer Scroll speed X", Float) = 1.0
	_Scroll2Y ("2nd layer Scroll speed Y", Float) = 0.0
	_AMultiplierCloud ("Cloud Multiplier", Float) = 0.5
	//_AMultiplierMonth ("Month Multiplier", Float) = 0.5
}

SubShader {
	Tags { "Queue"="Transparent" "RenderType"="Transparent" }
	
	Lighting Off 
	Fog { Mode Off }
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha
	LOD 100
	
		
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	sampler2D _DetailTex;
	//sampler2D _Month;

	float4 _MainTex_ST;
	float4 _DetailTex_ST;
	//float4 _Month_ST;
	
	float _ScrollX;
	float _ScrollY;
	float _Scroll2X;
	float _Scroll2Y;
	float _AMultiplierCloud;
	//float _AMultiplierMonth;
	
	struct v2f {
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;
		//float2 uv2 : TEXCOORD1;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(_ScrollX, _ScrollY) * _Time);
		o.uv.zw = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + frac(float2(_Scroll2X, _Scroll2Y) * _Time);
		//o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_Month);
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		half4 frag (v2f i) : COLOR
		{
			half4 o;
			half4 tex = tex2D (_MainTex, i.uv.xy);
			half4 tex2 = tex2D (_DetailTex, i.uv.zw);
			o.rgb = tex.rgb * tex2.rgb * _AMultiplierCloud;
			o.a = (tex.a + tex2.a) * 0.5;
			//fixed4 monTex = tex2D(_Month,i.uv2);
			//monTex.rgb *= _AMultiplierMonth;
			//if(monTex.a > 0)
			//	o = (tex * tex2) * _AMultiplierCloud + monTex;
			//else
			//{
			//	fixed4 c = (tex * tex2) * _AMultiplierCloud;
			//	o.rgb = monTex.rgb + (c.rgb - monTex.rgb) * monTex.a;
			//	o.a = c.a;
			//}
			
			return o;
		}
		ENDCG 
	}	
}
}
