Shader "MyMobile/VertexLit" {
Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Geometry" }
		LOD 50

		Pass {
			Lighting Off
			SetTexture [_MainTex] { 
				combine texture
			} 
		}
	}
}
