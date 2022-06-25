// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/Color/Blend_Alpha" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_AlphaCon("AlphaCon",Range(0,1)) = 1
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType" = "Opaque" }

	    Blend SrcAlpha OneMinusSrcAlpha
		Pass { 
		    Name "COLORBLEND"
		    CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    #pragma fragmentoption ARB_precision_hint_fastest
		    
		    half4 vert(float4 vertex : POSITION) : SV_POSITION
		    {
		    	 return UnityObjectToClipPos(vertex);
		    }
		    
		    half4 _Color;
		    half _AlphaCon;
		    
		    half4 frag() : COLOR
		    {
		        return half4(_Color.rgb,_AlphaCon);
		    }
		    ENDCG
       }
	} 
}
