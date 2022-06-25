// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Hard/Diffuse" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color",Color) = (0.5,0.5,0.5,0.5)
		_Expand("Expand",Range(0,1)) = 0.3
		_AlphaCon("AlphaCon",Range(0,1)) = 1
	}
	SubShader {
		Tags {"Queue"="Geometry+20" "RenderType"="Opaque"}
		LOD 200
		
		CGINCLUDE
		#include "UnityCG.cginc"
		
		struct v2f{
		    float4 pos : SV_POSITION;
		    //#if defined(USE_TEXTURE)
		    float2 uv : TEXCOORD;
		    //#endif
		    float vdotn : TEXCOORD1;
		};
		
		struct vertDate{
		    float4 vertex : POSITION;
		    //#if defined(USE_TEXTURE)
		    float4 texcoord : TEXCOORD;
		    //#endif
		    float3 normal : NORMAL;
		};
		
		half _Expand;
		
		v2f vert(vertDate v)
		{
			v2f o;
			float3 normal = normalize(v.normal);
			
			float4 vertex = v.vertex;
			//#if defined(USE_TEXTURE)
			o.uv = v.texcoord.xy;
			//#else
			//vertex.xyz += normal * _Expand;
			//#endif
			
			o.pos = UnityObjectToClipPos( vertex);
			float3 viewDirO = ObjSpaceViewDir(v.vertex);
			o.vdotn = dot(normalize(viewDirO),normal);
			o.vdotn = 1 - saturate(o.vdotn + _Expand);
			
			return o;
		}
		
		sampler2D _MainTex;
		half4 _Color;
		half _AlphaCon;
		half4 frag(v2f i) : COLOR
		{
			half4 outColor = half4(1,1,1,1);
			//#if defined(USE_TEXTURE)
			half4 texColor = tex2D(_MainTex,i.uv);
			half4 spaceCol = texColor * i.vdotn;
			//outColor.rgb = (texColor.rgb * 0.7 + texColor.rgb * _Color.rgb * 0.3 + 2 * spaceCol.rgb * texColor.a * _Color * (0.5 + 0.5 * _CosTime.w)) * 1.3;
			//outColor.rgb = (texColor.rgb +  3 * spaceCol.rgb  * _Color.rgb * (0.5 + 0.5 * cos(_Time.z)));
			outColor.rgb = (texColor.rgb +  3 * spaceCol.rgb  * _Color.rgb * (0.8 + 0.2 * cos(_Time.z)));
			outColor.a = texColor.a * _Color.a * _AlphaCon;
			//#else
			//outColor = _Color;
			//outColor.a *= (i.vdotn);
			//outColor = outColor  * (0.9 + 0.1 * _CosTime.x);
			//#endif
			return outColor;
		}
		
		ENDCG
		
		
		//pass{
		//ZWrite off
		//Blend SrcAlpha OneMinusSrcAlpha
		//CGPROGRAM
		//#pragma vertex vert
		//#pragma fragment frag
		//#pragma fragmentoption ARB_precision_hint_fastest

		//ENDCG
        //}
        
        pass{
        Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile USE_TEXTURE
		ENDCG
        }
            
	} 
}
