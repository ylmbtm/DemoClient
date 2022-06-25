Shader "MyMobile/Transparent/VertexDF" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Alpha("Alpha",Color) = (1,1,1,1)
		_AlphaCon("AlphaCon",Range(0,1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent"}
		cull off
		Lighting off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass{
			SetTexture[_MainTex]{
				constantColor[_Alpha]
				combine texture * constant
			}
			
			SetTexture[_MainTex]{
				constantColor(1,1,1,[_AlphaCon])
				combine Previous * constant
			}
		}

	} 
}
