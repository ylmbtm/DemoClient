// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Sky/GrayscaleToAlpha" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_ScrollX ("Base layer Scroll speed X", Float) = 1.0
	_ScrollY ("Base layer Scroll speed Y", Float) = 0.0
	_Scroll2X ("2nd layer Scroll speed X", Float) = 1.0
	_Scroll2Y ("2nd layer Scroll speed Y", Float) = 0.0
	_AMultiplierCloud ("Cloud Multiplier", Float) = 0.5
	_GrayscaleToAlpha("GrayscaleToAlpha",Range(0,1)) = 0
	_Contrast("Contrast",Float) = 1
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

	float4 _MainTex_ST;
	float4 _DetailTex_ST;
	
	float _ScrollX;
	float _ScrollY;
	float _Scroll2X;
	float _Scroll2Y;
	float _AMultiplierCloud;
	half _GrayscaleToAlpha;
	half _Contrast;
	
	struct v2f {
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(_ScrollX, _ScrollY) * _Time);
		o.uv.zw = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + frac(float2(_Scroll2X, _Scroll2Y) * _Time);
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
			
			if(_GrayscaleToAlpha)
			{
				tex.a = (tex.r + tex.g + tex.b) / 3;
				tex2.a = (tex2.r + tex2.g + tex2.b) / 3;
			}
			o.a = (tex.a + tex2.a) * 0.5;

			half t = (1 - _Contrast) * 0.5;
			o.rgb = o.rgb * _Contrast + t;
			
			return o;
		}
		ENDCG 
	}	
}
}
