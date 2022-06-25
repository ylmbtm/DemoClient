
Shader "MyMobile/Lightmap Alpha" {
Properties {
    _AlphaCon ("AlphaCon", Range(0,1)) = 0
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
    Tags { "Queue"="Transparent" "RenderType"="Transparent" }
	LOD 50
	
	Pass {
		Tags { "LightMode" = "Vertex" }
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		SetTexture [_MainTex] {
		 constantColor (1,1,1,[_AlphaCon])
		 combine texture * constant
		 } 
	}
	
	Pass {
		Tags { "LightMode" = "VertexLM" }
		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 
			Bind "texcoord", texcoord1 
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture
		}
		SetTexture [_MainTex] {
			combine texture * previous DOUBLE, texture * primary
		}
	}
	
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }
		
		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 
			Bind "texcoord", texcoord1 
		}
		
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [_MainTex] {
			combine texture * previous QUAD, texture * primary
		}
	}	
}
}



