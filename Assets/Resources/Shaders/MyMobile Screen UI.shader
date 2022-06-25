// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Screen/UI" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color",Color) = (0,0,0,1)
	}
	SubShader {
		Tags {"Queue"="Transparent+100" "RenderType"="Transparent" }
		
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_percision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;
			
			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD;
			};
			
			struct v_date{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
			};
			
			v2f vert(v_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				half texAlpha = tex2D(_MainTex,i.uv).a;
				half4 outColor;
				outColor.rgb = _Color.rgb;
				outColor.a = _Color.a * texAlpha;
				return outColor;
			}
			

			ENDCG
		}
	} 
}
