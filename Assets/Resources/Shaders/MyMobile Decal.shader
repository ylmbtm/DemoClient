// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Decal" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DecalTex("Decal(RGBA)",2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 50
		
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			sampler2D _MainTex;
			sampler2D _DecalTex;

			struct vert_date {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
			};
			
			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD;
			};

			v2f vert(vert_date v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 texColor = tex2D(_MainTex,i.uv);
				fixed4 decalTex = tex2D(_DecalTex,i.uv);
				fixed4 outColor = fixed4(1,1,1,1);
				outColor.rgb = texColor.rgb - (texColor.rgb - decalTex.rgb) * decalTex.a;
				return outColor;
			}
			ENDCG
		}
	} 
}
