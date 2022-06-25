// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "MyMobile/VertexLit/Diffuse_Shadow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ShadowTex("ShadowTex",2D) = "white" {}
		_ShadowColor("ShadowColor",Color) = (1,1,1,0.5)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 50

		Lighting Off
		
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			// float4 unity_LightmapST;
			// sampler2D unity_Lightmap;

			uniform float4x4 _GlobalProjector;
			uniform sampler2D _ShadowTex;
			float4 _ShadowColor;

			struct vert_date {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
				float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD;
				float2 lightMapUV : TEXCOORD1;
				
				float4 uvShadow : TEXCOORD2;
			};

			v2f vert(vert_date v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.lightMapUV = ((v.texcoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
				o.uv = v.texcoord.xy;

				o.uvShadow = mul(_GlobalProjector, v.vertex);

				return o;

			}
			
			half4 frag(v2f i) : COLOR
			{
			    half3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightMapUV));
				half4 texColor = tex2D(_MainTex,i.uv);
				texColor.rgb *= lightmap;


				half4 shadowTex = tex2Dproj(_ShadowTex,i.uvShadow);
				shadowTex.a = (1 - shadowTex.a * _ShadowColor.a);
				shadowTex.rgb = _ShadowColor.rgb * shadowTex.a;
				
				texColor *= shadowTex;
				return texColor;
			}
			
			ENDCG
		}
	} 
}
