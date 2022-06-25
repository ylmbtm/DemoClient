Shader "MyMobile/Multiply/Vert_Forward" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		//_Alpha("Alpha",Range(0,1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent-100" "RenderType"="Opaque"}
		Blend Zero SrcColor
		//Blend SrcAlpha OneMinusSrcAlpha
		//cull off
		Lighting Off
		ZWrite off
		Fog { Color (1,1,1,1) }
		Pass{
			SetTexture[_MainTex]{
				//constantColor(1,1,1,[_Alpha])
				//combine texture * constant
				combine texture
			}
		}
	} 
}
