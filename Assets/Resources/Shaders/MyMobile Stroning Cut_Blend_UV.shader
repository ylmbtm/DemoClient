// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Stroning/Cut_Blend_UV" {
	Properties {
		_MainTex ("Main", 2D) = "white" {}
		_ScrollMainTexX("ScrollMainTexX",float) = 0
		_ScrollMainTexY("ScrollMainTexY",float) = 0
		_DetailTex("DetailTex",2D) = "while"{}
		_ScrollDetailTexX("ScrollDetailTexX",float) = 0
		_ScrollDetailTexY("ScrollDetailTexY",float) = 0
		_Percent ("Percent", Range(0,1)) = 0
	}
	SubShader {
		Blend SrcAlpha OneMinusSrcAlpha
	    pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD;
			float2 uv1 : TEXCOORD1;
		};
		
		struct my_date{
		    float4 vertex : POSITION;
		    float4 texcoord : TEXCOORD;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _DetailTex;
		float4 _DetailTex_ST;
		float _ScrollMainTexX,_ScrollMainTexY;
		float _ScrollDetailTexX,_ScrollDetailTexY;
		half _Percent;
		
		v2f vert(my_date v)
		{
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
			o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex) + frac(float2(_ScrollMainTexX, _ScrollMainTexY) * _Time);
			o.uv.zw = v.texcoord.xy;

			o.uv1 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + frac(float2(_ScrollDetailTexX, _ScrollDetailTexY) * _Time);

		    return o;
		}
	
		half4 frag(v2f o) : COLOR
		{
		    half4 maint = tex2D(_MainTex,o.uv.xy);
			half4 detail = tex2D(_DetailTex,o.uv1);
		    half4 outcolor;
		    if(o.uv.z > 1 - _Percent)
		    {
		        outcolor = maint + detail * detail.a;
		    }
		    else
		    {
		        outcolor = fixed4(0,0,0,0);
		    }
		    return outcolor;
		}
		
		ENDCG
	} 
	}
}
