// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/VertexLit/TwoTextureAdd_NOFOG" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MoonGlow("Glow",2D) = "black"{}
		_GlowColor("Color",Color) = (0.5,0.5,0.5,0.5)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Fog{mode off}
		LOD 50
		
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			//#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MoonGlow;
			half4 _GlowColor;
			//float4 unity_LightmapST;
			//sampler2D unity_Lightmap;
			

			struct vert_date {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
				//float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD;
				//float2 lightMapUV : TEXCOORD1;			
			};

			v2f vert(vert_date v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				//o.lightMapUV = ((v.texcoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
				o.uv = v.texcoord.xy;
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
			    //half3 lightmap = DecodeLightmap(tex2D(unity_Lightmap, i.lightMapUV));
				half4 texColor = tex2D(_MainTex,i.uv);
				//texColor.rgb *= lightmap;
				half4 moonColor = tex2D(_MoonGlow,i.uv) * _GlowColor;
				half4 outColor = moonColor * moonColor.a + texColor;
				return outColor;
			}
			
			ENDCG
		}
	} 
}
