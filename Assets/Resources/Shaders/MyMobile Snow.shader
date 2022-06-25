// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "MyMobile/FX/Snow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	CGINCLUDE
		sampler2D _MainTex;		
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;		
		};
		struct app_date{
			float4 vertex : POSITION;
			float4 texcoord : TEXCOORD;
		};

		v2f vert(app_date v)
		{
			v2f o;
			o.uv.xy = v.texcoord.xy;
			o.pos = UnityObjectToClipPos (v.vertex);	
						
			return o; 
		}
		
		half4 frag( v2f i ) : COLOR
		{	
			return tex2D(_MainTex, i.uv);
		}
	
	ENDCG
	
	SubShader {
		Tags { "Queue"="Transparent"  "Queue" = "Transparent" }
		Cull Off
		ZWrite Off
		Fog {color(0,0,0,0)}
		Blend SrcAlpha One
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
