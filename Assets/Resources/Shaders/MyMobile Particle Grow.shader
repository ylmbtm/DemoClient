// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Particle/Grow" {
	Properties {
		_MainTex ("Base texture", 2D) = "white" {}
		_FadeOutDistNear ("Near fadeout dist", float) = 10	
		_FadeOutDistFar ("Far fadeout dist", float) = 10000	
		_BlinkingTimeOffsScale("Blinking time offset scale (seconds)",float) = 5
		_Multiplier("Color multiplier", float) = 1
		_Color("Color", Color) = (1,1,1,1)
	}

		
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend One One
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog { Color (0,0,0,0) }
		
		LOD 100
		CGINCLUDE	
		#include "UnityCG.cginc"
		sampler2D _MainTex;
		
		float _FadeOutDistNear;
		float _FadeOutDistFar;
		float _BlinkingTimeOffsScale;
		float _Multiplier;
		
		float4 _Color;
		
		
		struct v2f {
			float4	pos	: SV_POSITION;
			float2	uv		: TEXCOORD0;
			half4	color	: TEXCOORD1;
		};

		
		v2f vert (appdata_full v)
		{
			v2f 	o;	
			float	time 		= _Time.y + _BlinkingTimeOffsScale;
			float3	viewPos		= mul(UNITY_MATRIX_MV,v.vertex);
			float	dist		= length(viewPos);
			float	nfadeout	= saturate(dist / _FadeOutDistNear);
			float	ffadeout	= 1 - saturate(max(dist - _FadeOutDistFar,0) * 0.2);
			float	fracTime	= fmod(time,2);
			float	noiseTime	= time * 6.2831853f;
			float	noise		= sin(noiseTime) * (0.5f * cos(noiseTime * 0.6366f + 56.7272f) + 0.5f);
			float	noiseWave	= 0.5 * noise + 0.5;
			
			ffadeout *= ffadeout;

			nfadeout *= nfadeout;
			nfadeout *= nfadeout;
			
			nfadeout *= ffadeout;
			
			float4	mdlPos = v.vertex;
					
			o.uv	= v.texcoord.xy;
			o.pos	= UnityObjectToClipPos(mdlPos);
			o.color	= nfadeout * _Color * _Multiplier * noiseWave;
							
			return o;
		}
		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest		
			half4 frag (v2f i) : COLOR
			{		
				return tex2D (_MainTex, i.uv.xy) * i.color;
			}
			ENDCG 
		}	
	}
}
