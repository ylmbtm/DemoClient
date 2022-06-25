// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "MyMobile/FX/RainSplash" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Intensity ("Intensity", Range (0.5, 4.0)) = 1.5
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		
		uniform half4 _MainTex_ST;
		uniform half4 _Color;
		uniform half4 _CamUp;
		
		uniform half _Intensity;
		
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;	
			half4 color : TEXCOORD1;
		};

		v2f vert(appdata_full v)
		{
			v2f o;
			
			half timeVal = frac(_Time.z * 0.5 + v.texcoord1.x) * 2.0;
			
			o.uv.xy = v.texcoord.xy;
			
			// animation of 6 frames:
			o.uv.x = o.uv.x / 6 + floor(timeVal * 6) / 6;
			
			o.pos = UnityObjectToClipPos (v.vertex);	
			
			o.color = saturate(1.0 - timeVal) * _Intensity;		
			
			return o; 
		}
		
		half4 frag( v2f i ) : COLOR
		{	
			half4 outColor = tex2D(_MainTex, i.uv) * i.color;
			return outColor;
		}
	
	ENDCG
	
	SubShader {
		Tags { "Queue" = "Transparent" }
		Cull Off
		ZWrite Off
		Fog {color(0,0,0,0)}
		Blend One OneMinusSrcColor

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
