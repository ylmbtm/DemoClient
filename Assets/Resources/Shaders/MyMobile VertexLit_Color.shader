Shader "MyMobile/VertexLit_Color" {
Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Geometry" }
		LOD 50

		Pass {
			Lighting Off
			SetTexture [_MainTex] { 
				ConstantColor[_Color]
				combine texture * constant
			} 
		}
	}
}
