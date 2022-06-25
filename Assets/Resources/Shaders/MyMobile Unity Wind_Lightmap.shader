// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "MyMobile/Unity/Wind_Lightmap" {
Properties {
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Wind("Wind params",Vector) = (1,1,1,1)
	_WindEdgeFlutter("Wind edge fultter factor", float) = 0.5
	_WindEdgeFlutterFreqScale("Wind edge fultter freq scale",float) = 0.5
	_AlphaCon("AlphaCon",Range(0,1)) = 1
}

SubShader {
	Tags {"Queue"="Transparent" "RenderType"="Transparent" "LightMode"="ForwardBase"}
	LOD 100
	
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Off 
	ZWrite Off
	
	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TerrainEngine.cginc"
	sampler2D _MainTex;
	float4 _MainTex_ST;
	samplerCUBE _ReflTex;
	
	// float4 unity_LightmapST;
	// sampler2D unity_Lightmap;
	
	float _WindEdgeFlutter;
	float _WindEdgeFlutterFreqScale;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 lmap : TEXCOORD1;
		half3 spec : TEXCOORD2;
	};

inline float4 AnimateVertex2(float4 pos, float3 normal, float4 animParams,float4 wind,float2 time)
{	
	float fDetailAmp = 0.1f;
	float fBranchAmp = 0.3f;

	float fObjPhase = dot(unity_ObjectToWorld[3].xyz, 1);
	float fBranchPhase = fObjPhase + animParams.x;
	
	float fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);
	float2 vWavesIn = time  + float2(fVtxPhase, fBranchPhase );
	float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);
	
	vWaves = SmoothTriangleWave( vWaves );
	
	float2 vWavesSum = vWaves.xz + vWaves.yw;
	float3 bend = animParams.y * fDetailAmp * normal.xyz;
	
	bend.y = animParams.w * fBranchAmp;
	pos.xyz += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w; 
	pos.xyz += animParams.z * wind.xyz;
	
	return pos;
}


	
	v2f vert (appdata_full v)
	{
		v2f o;
		
		float4 wind;
		float bendingFact = v.color.a;
		
		wind.xyz	= mul((float3x3)unity_WorldToObject,_Wind.xyz);
		wind.w		= _Wind.w  * bendingFact;
		
		float4	windParams	= float4(0,_WindEdgeFlutter,bendingFact.xx);
		float 	windTime 	= _Time.y * float2(_WindEdgeFlutterFreqScale,1);
		float4	mdlPos		= AnimateVertex2(v.vertex,v.normal,windParams,wind,windTime);
		
		o.pos  = UnityObjectToClipPos(mdlPos);
		o.uv   = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.spec = v.color;
		o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma debug
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		
		fixed _AlphaCon;
		
		half4 frag (v2f i) : COLOR
		{
			half4 tex = tex2D (_MainTex, i.uv);
			
			half4 c;
			c.rgb = tex.rgb;
			c.a = tex.a * _AlphaCon;
			
			half3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
			c.rgb *= lm;
			
			return c;
		}
		ENDCG 
	}	
}
}
