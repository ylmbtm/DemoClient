// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Cloud/Blend_Mask" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask(RGB)",2D) = "white"{}
		_CloudColor("CloudColor",Color) = (1,1,1,1)
		_FadeOutDistNear ("Near fadeout dist", float) = 10	
		_FadeOutDistFar ("Far fadeout dist", float) = 10000
		_UvOffset ("UVOffset(XY-Texture1,ZW-Texture2)",Vector) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MaskTex;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _FadeOutDistNear;
			half _FadeOutDistFar;
			half4 _UvOffset;
			half4 _CloudColor;
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
				float2 uv1 :TEXCOORD1;
			};
			
			struct base_date{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			v2f vert(base_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord0,_MainTex);
				o.uv1 = v.texcoord0.xy;
				half4 offset = _UvOffset * _Time.y;
				o.uv.xy += offset.xy;
				
				float4 viewPos = mul(UNITY_MATRIX_MV,v.vertex);
				float dist = -viewPos.z;
				float nfadeout = saturate(dist / _FadeOutDistNear);
				float ffadeout = 1 - saturate(max(dist - _FadeOutDistFar,0) * 0.2);
				
				ffadeout *= ffadeout;
				nfadeout *= nfadeout;
				nfadeout *= nfadeout;
				nfadeout *= ffadeout;
				
				o.uv.z = nfadeout;
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				half texAlpha = tex2D(_MainTex,i.uv.xy).a;
				half maskAlpha = tex2D(_MaskTex,i.uv1).a;
				half4 outColor = _CloudColor;
				outColor.a = outColor.a * texAlpha * maskAlpha * i.uv.z;
				return outColor;
			}
			ENDCG
		}
	}
}
