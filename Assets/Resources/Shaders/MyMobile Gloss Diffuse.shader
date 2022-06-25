// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Gloss/Diffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GlossColor("GlossColor",Color) = (1,1,1,1)
		_Shininess ("Shininess", Range (0.01, 40)) = 0.078125
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 50
		Cull Back
		ZWrite on
		offset 0,0
		
		pass{
		    CGPROGRAM
	    	#pragma vertex vert
	    	#pragma fragment frag
	    	#pragma fragmentoption ARB_precision_hint_fastest
	    	#include "UnityCG.cginc"

	    	sampler2D _MainTex;
	    	half4 _GlossColor;
	    	half _Shininess;

	    	struct v2f {
	    		float4 pos : SV_POSITION;
	    		float2 uv0 : TEXCOORD0;
	    		float3 viewDir : TEXCOORD1;
	    		float3 lightDir : TEXCOORD2;
	    		float3 normal : TEXCOORD3;
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
		        o.normal = v.normal;
		        o.uv0 = v.texcoord0.xy;
		        
		        o.viewDir = WorldSpaceViewDir(v.vertex);
		        o.lightDir = WorldSpaceLightDir(v.vertex);
		        
		        o.viewDir = ObjSpaceViewDir(v.vertex);
		        o.lightDir = ObjSpaceLightDir(v.vertex);
		        
		        return o;
		    }
		    
		    half4 frag(v2f o) : COLOR
		    {
		        half4 texColor = tex2D(_MainTex,o.uv0);
		        
		        half3 viewDir = normalize(o.viewDir);
		        half3 lightDir = normalize(o.lightDir);
		        half3 normal = o.normal;
		        
		        half s = max(0,dot(lightDir,normal));
		        half3 h = normalize(viewDir + lightDir);
		        half r = max(0,dot(normal,h));
		        half spec = pow(r,_Shininess);
		        
		        half4 outColor ;
		        outColor.rgb = (texColor.rgb + spec * _GlossColor.rgb * texColor.a);
		        outColor.a = texColor.a;
		        
		        return outColor;
		    }
	    	ENDCG
	    } 
	}
}
