// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/StroningCut" {
	Properties {
		_MainTex ("Main", 2D) = "white" {}
		_MaskTex ("Mask", 2D) = "while" {}
		_Percent ("Percent", Range(0,1)) = 0
	}
	SubShader {
	    pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD;
		};
		
		struct my_date{
		    float4 vertex : POSITION;
		    float2 texcoord : TEXCOORD;
		};

		v2f vert(my_date v)
		{
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
		    o.uv = v.texcoord;
		    return o;
		}
		
		sampler2D _MainTex;
		sampler2D _MaskTex;
		fixed _Percent;
		
		half4 frag(v2f o) : COLOR
		{
		    half4 maint = tex2D(_MainTex,o.uv);
		    half4 maskt = tex2D(_MaskTex,o.uv);
		    
		    half4 outcolor;
		    if(o.uv.y > _Percent)
		    {
		        outcolor = maint;
		    }
		    else
		    {
		        outcolor = maskt;
		    }
		    return outcolor;
		}
		
		ENDCG
	} 
	}
	FallBack "Diffuse"
}
