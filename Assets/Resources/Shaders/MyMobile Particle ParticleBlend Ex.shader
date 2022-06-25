// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Particles/ParticleBlend" {
Properties {
	_Alpha ("Alpha", Color) = (0.5,0.5,0.5,0.5)
	_Color ("Color", Color) = (1,1,1,1)
	_AlphaCon("AlphaCon",Range(0,1)) = 1
	_MainTex ("Particle Texture", 2D) = "white" {}
	_MaskTex("Mask",2D) = "white"{}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask RGB
	Cull Off 
	Lighting Off 
	ZWrite Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

			sampler2D _MainTex;
			sampler2D _MaskTex;
			half4 _Alpha;
			half _AlphaCon;
			half4 _Color;
			
			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			float4 _MainTex_ST;
			float4 _MaskTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord1,_MaskTex);
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 mainTex = tex2D(_MainTex, i.texcoord);
				half4 maskTex = tex2D(_MaskTex, i.texcoord1);
				half alpha = mainTex.a * maskTex.a;
				half4 outColor = fixed4(mainTex.rgb,alpha);
				
				outColor = 2.0f * i.color * _Alpha * outColor * _Color;
				outColor.a *= _AlphaCon;
				return outColor;
			}
			ENDCG 
		}
	} 	
}
}