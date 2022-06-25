// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MyMobile/Water/Diffuse" {
	Properties {
		_MainTex0 ("Texture1", 2D) = "white" {}
		_MainTex1 ("Texture2", 2D) = "white" {}
		_RefractTex("RefracTex",2D) = "white" {}
		_UvOffset ("UVOffset(XY-Texture1,ZW-Texture2)",Vector) = (1,1,1,1)
	}
	SubShader {
		Tags {"RenderType"="Opaque" }
		LOD 50
		
		
		pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"
		
		struct v2f{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			float2 uv2 : TEXCOORD2;
			float3 normalWorld : TEXCOORD3;
			float3 viewDirWorld : TEXCOORD4;
		};
		
		
		sampler2D _MainTex0;
		sampler2D _MainTex1;
		sampler2D _RefractTex;
		float4 _MainTex0_ST;
		float4 _MainTex1_ST;
		float4 _RefractTex_ST;
		
		fixed4 _UvOffset;
		
		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			float4 uvoffset = _Time.x * _UvOffset;
			o.uv = TRANSFORM_TEX(v.texcoord,_MainTex0);
			o.uv += uvoffset.xy;
			o.uv1 = TRANSFORM_TEX(v.texcoord,_MainTex1);
			o.uv1 += uvoffset.zw;
			o.uv2 = TRANSFORM_TEX(v.texcoord,_RefractTex);
			
			float4 normalWorld= mul(float4(v.normal,0),unity_ObjectToWorld);
			o.normalWorld = normalWorld.xyz;
			o.viewDirWorld = -WorldSpaceViewDir(v.vertex);
			return o;
		}
		
		half4 frag(v2f i) : COLOR
		{
			half4 mainTex0 = tex2D(_MainTex0,i.uv);
			float2 uv1 = 1 - i.uv1;
			half4 mainTex1 = tex2D(_MainTex1,uv1);
			half eta = dot(mainTex0,mainTex1);
			
			half3 ref = refract(i.viewDirWorld,i.normalWorld,eta);
			float4 refuv = float4(ref.xz,0,0);
			float2 uvoffset = TRANSFORM_TEX(refuv,_RefractTex);
			half4 refractTex = tex2D(_RefractTex,uvoffset);
			//float2 uvoffset = i.uv2 * eta;
			//fixed4 refractTex = tex2D(_RefractTex,uvoffset);
			
			half4 outColor = mainTex0 * mainTex1;
			outColor = outColor * 1.3 + outColor * refractTex * 2;
			return outColor;
		}
		
		ENDCG
		}
	}
}
