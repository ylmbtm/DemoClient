// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "MyMobile/Tree Cut" {
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="False"}
		LOD 50
		Pass
		{	
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"
		
		    // uniform sampler2D unity_Lightmap;
		    // float4 unity_LightmapST;
			sampler2D _MainTex;
			half _Cutoff;
			
			struct data_simple 
			{
			    float4 vertex : SV_POSITION;
			    float4 texcoord0 : TEXCOORD0;
			    float4 texcoord1 : TEXCOORD1;
			};
			struct v2f
			{
				float4 pos : POSITION;
				float4 uv0 : TEXCOORD0;
				float2 lightmapUV : TEXCOORD1;
			};
			struct out_depth 
			{
				float4 col:COLOR;
				float dep:DEPTH;
			};      

			v2f vert(data_simple v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv0.xy = v.texcoord0.xy;
				o.uv0.zw = o.pos.zw;
				o.lightmapUV = ((v.texcoord1.xy * unity_LightmapST.xy) + unity_LightmapST.zw);
				return o; 
			}
			out_depth frag(v2f i)
			{
				out_depth o;
				o.col = tex2D(_MainTex, i.uv0.xy);
				half3 lightmapColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap,i.lightmapUV));
				o.col.rgb = o.col.rgb * lightmapColor;
				o.col.a = max(o.col.a - _Cutoff, 0.0);
				
				if(o.col.a <= 0)
				{
					o.col.a = 0;
				    o.dep = 1;
				}
				else
				{
					o.col.a = 1;
					o.dep = i.uv0.z / i.uv0.w;
				}
				return o;
			}
			ENDCG
		}
	}
}
