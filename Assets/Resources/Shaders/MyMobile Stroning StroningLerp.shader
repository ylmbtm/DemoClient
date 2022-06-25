// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Stroning/StroningLerp" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		_MaskTex ("MaskTex", 2D) = "while" {}
		_StronColor("StronColor",Color) = (1,1,1,1)
		_Percent ("Percent", Range(0,1)) = 0
	}
	SubShader {	
		pass{
		Name "STRONBLEND"
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD;
		};
		
		struct my_date{
		    float4 vertex : POSITION;
		    float2 texcoord : TEXCOORD;
		};
		
		
		v2f vert(my_date v)
		{
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
		    o.uv = v.texcoord;
		    return o;
		}
		
		sampler2D _MainTex;
		sampler2D _MaskTex;
		half _Percent;
		half4 _StronColor;
		
		half4 frag(v2f o):COLOR
		{
		    half4 maintex = tex2D(_MainTex,o.uv);
		    half4 masktex = tex2D(_MaskTex,o.uv) * _StronColor;
		    
		    half4 outcolor = lerp(maintex,masktex,_Percent);
		    return outcolor;
		}
		
		ENDCG
		}
	} 
}
