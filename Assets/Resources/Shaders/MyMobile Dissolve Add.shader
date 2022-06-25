// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Dissolve/Add" {
	Properties {
	    _MainColor("MainColor",Color) = (1,1,1,1)
		_MainTex ("Main", 2D) = "white" {}
		_DissolveTex ("DissolveTex", 2D) = "white" {}
		_Mask ("Mask",2D) = "white" {}
		_Dissolve ("Dissove",Range(-0.01,1.0)) = 0.0
		_DissolveSpeed ("Dissove Speed",Range(1.0,5.0)) = 5.0
	}
	SubShader {
	Tags { "Queue"="Transparent+40" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Cull off
	ZWrite off
	LOD 50
	pass{
	
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"
        
		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD;
			float2 uv1 : TEXCOORD1;
		};
		
		struct my_date{
		    float4 vertex : POSITION;
		    float4 texcoord : TEXCOORD;
		    float2 texcoord1 : TEXCOORD1;
		};

        half4 _MainTex_ST;
        half4 _DissolveTex_ST;
		v2f vert(my_date v)
		{
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
		    o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
		    o.uv1 = v.texcoord1;
		    return o;
		}
		
		half4 _MainColor;
		sampler2D _MainTex;
		sampler2D _DissolveTex;
		sampler2D _Mask;
		half _Dissolve;
		half _DissolveSpeed;
		
		half4 frag(v2f o) : COLOR
		{
		    half4 disst = tex2D(_DissolveTex,o.uv1);
			_MainColor.a = 1;
		    half4 maint = tex2D(_MainTex,o.uv) * _MainColor;
		    half4 maskt = tex2D(_Mask,o.uv1);
		    
		    half4 outcolor;
		    outcolor = lerp(half4(0,0,0,0),maint,maskt);
		    if(disst.a <= _Dissolve)
            {
		        half alpha = saturate(outcolor.a - _Dissolve * 2 + disst.a);
		        alpha = pow(alpha,_DissolveSpeed);
		        outcolor.a = alpha;
		    }
		    
		    return outcolor;
		}
		ENDCG
	} 
	}
}
