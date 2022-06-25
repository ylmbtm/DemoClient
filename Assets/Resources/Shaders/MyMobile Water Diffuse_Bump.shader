// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Water/Diffuse_Bump" {
	Properties {
		_Color("WaterColor",Color) = (0,0.2,0.75,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MaskMap("MaskMap",2D) = "white" {}
		_UvOffset ("UVOffset(XY-Texture1,ZW-Texture2)",Vector) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Geometry" "RenderType"="Opaque" }
		pass{
			//Tags{"LightMode"="ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MaskMap;
			half4 _MainTex_ST;
			half4 _MaskMap_ST;
			half4 _UvOffset;
			half4 _Color;
			
			struct bump_date {
				float4 vertex : POSITION;
			    float4 texcoord : TEXCOORD0;
			};
			
			struct v2f{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			
			v2f vert(bump_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord,_MaskMap);
				
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				float4 offset_uv = _UvOffset * _Time.y;
				
				float2 uvMask = i.uv.zw + offset_uv.xy;
				half4 mask = tex2D(_MaskMap,uvMask);
				half normalVal = dot(mask.rgb,mask.rgb);
				float2 uv = i.uv.xy * normalVal;
				half4 texWithBormal = tex2D(_MainTex,uv);
				
				float2 uvMask1 = (1 - i.uv.zw) + offset_uv.zw;
				half4 mask1 = tex2D(_MaskMap,uvMask1);
				half normalVal1 = dot(mask1.rgb,mask1.rgb);
				float2 uv1 = i.uv.xy * normalVal1;
				half4 texWithBormal1 = tex2D(_MainTex,uv1);
				
				half4 outColor = texWithBormal * texWithBormal1 * 2;
				outColor.rgb = (outColor.rgb + 0.5) * (outColor.rgb + 0.5) - 0.5;
				
				outColor *= _Color;
				
				return outColor;
			}
			
			ENDCG
		}
	} 
}
