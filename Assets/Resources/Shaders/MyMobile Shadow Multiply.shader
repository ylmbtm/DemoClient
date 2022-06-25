// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Shadow/Multiply" {
	Properties {
		_ShadowTex ("Base (RGB)", 2D) = "white" {}
		_ShadowColor("ShadowColor",Color) = (1,1,1,0.5)
	}
	SubShader {
		Tags { "Queue"="Transparent+100" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend Zero SrcColor
		Lighting Off
		ZWrite off
		Offset -1,-1
		Fog { Color (1,1,1,1) }
		
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			
			float4x4 _GlobalProjector;
			sampler2D _ShadowTex;
			float4 _ShadowColor;
			
			struct v2f{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD;
			};
			
			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uv = mul(_GlobalProjector, vertex);
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				half4 texColor = tex2Dproj(_ShadowTex,i.uv);
				texColor.a = (1 - texColor.a * _ShadowColor.a);
				texColor.rgb = _ShadowColor.rgb * texColor.a;
				return texColor;
			}
			ENDCG
		}
		}
}
