
Shader "MyMobile/Lightmap" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_HitColor("Hit (RGB)",Color) = (0,0,0,0)
}

SubShader {
	Tags { "Queue"="Geometry+100" "RenderType"="Opaque" }
	LOD 50
	
	Pass {
		Tags { "LightMode" = "Vertex" }
		Lighting Off
		SetTexture [_MainTex] { 
		    constantColor[_HitColor]
		    combine texture + Constant
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



