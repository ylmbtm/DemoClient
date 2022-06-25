Shader "MyMobile/Monster/Transparent_VertLit" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlphaCon ("AlphaCon", Range(0,1)) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		
		pass{
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
		    SetTexture[_MainTex]{
		    	constantColor(1,1,1,[_AlphaCon])
		    	combine texture * constant
		    }
		}
	} 
}
