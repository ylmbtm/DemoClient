Shader "MyMobile/Avatar AlphaVertex" {
    Properties {
	  _AlphaCon ("AlphaCon", Range(0,1)) = 1
      _MainTex ("Texture", 2D) = "white" {}
    }

	
    SubShader {
	   Tags {"Queue"="Transparent" "RenderType" = "Opaque" }
	   LOD 50

	   Blend SrcAlpha OneMinusSrcAlpha
	   ZWrite off
	   Pass { 
	      Name "VERTEXLI"
	     
	      SetTexture[_MainTex]{
	      	  constantColor(1,1,1,[_AlphaCon])
	          combine texture,texture * constant
	      }
       }
    } 
}
