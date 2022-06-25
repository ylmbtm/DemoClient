// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MyMobile/Reflect/Add" {
	Properties {
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
		_AlphaCon("AlphaCon",Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 50
		Cull Back
		
		Blend SrcAlpha One
		
		Pass{
		    CGPROGRAM
	    	#pragma vertex vert
	    	#pragma fragment frag
	    	#pragma fragmentoption ARB_precision_hint_fastest
	    	#include "UnityCG.cginc"

            samplerCUBE _Cube;
	    	sampler2D _MainTex;
	    	fixed4 _ReflectColor;
	    	fixed _AlphaCon;

	    	struct v2f {
	    		float4 pos : SV_POSITION;
	    		float2 uv0 : TEXCOORD0;
	    		float3 ref : TEXCOORD2;
	    	};
	    	
	    	struct V2F_Data{
	    	    float4 vertex : POSITION;
	    	    float4 texcoord0 : TEXCOORD0;
	    	    float3 normal : NORMAL;
	    	};

		    v2f vert(V2F_Data v)
		    {
		        v2f o;
		        o.pos = UnityObjectToClipPos(v.vertex);
		        o.uv0 = v.texcoord0.xy;
		        
		        float3 N = mul(unity_ObjectToWorld,float4(v.normal,0)).rgb;
		        N = normalize(N);
		        float3 I = WorldSpaceViewDir(v.vertex);
		        
		        o.ref = reflect(I,N);
		        
		        return o;
		    }
		    
		    half4 frag(v2f o) : COLOR
		    {
		        half4 texColor = tex2D(_MainTex,o.uv0);
		        half4 refColor = texCUBE(_Cube,o.ref) * _ReflectColor;
		        
		        refColor.rgb = refColor.rgb * refColor.a;
		        
		        half4 outColor ;
		        outColor.rgb = (texColor.rgb + refColor.rgb * texColor.a);
		        outColor.a = saturate(texColor.a * _AlphaCon + refColor.a);
		        
		        return outColor;
		    }
			ENDCG
		}
	}
}

