// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Stroning/Cut_Blend" {
	Properties {
		_MainTex ("Main", 2D) = "white" {}
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
		};
		
		struct my_date{
		    float4 vertex : POSITION;
		    float4 texcoord : TEXCOORD;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		half _Percent;
		
		
		
		v2f vert(my_date v)
		{
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
		    o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
		    o.uv.zw = v.texcoord.xy;
		    return o;
		}
	
		half4 frag(v2f o) : COLOR
		{
		    half4 maint = tex2D(_MainTex,o.uv.xy);
		    half4 outcolor;
		    if(o.uv.z > 1 - _Percent)
		    {
		        outcolor = maint;
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
