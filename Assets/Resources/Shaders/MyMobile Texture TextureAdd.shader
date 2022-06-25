Shader "MyMobile/Texture/TextureAdd" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Geometry+110" "RenderType"="Opaque" }
		LOD 20
		Blend SrcAlpha One
		Pass{
		    SetTexture[_MainTex]{
		        constantColor[_Color]
		    	combine texture * constant DOUBLE
		    }
		}
	} 
}
