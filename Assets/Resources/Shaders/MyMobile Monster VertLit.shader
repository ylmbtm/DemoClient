Shader "MyMobile/Monster/VertLit" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_HitColor("Hit (RGB)",Color) = (0,0,0,0)
	}
	SubShader {
		Tags {"Queue"="Geometry+100" "RenderType"="Opaque" }
		
		pass{
			Lighting Off
		    SetTexture[_MainTex]{
		    	constantColor[_HitColor]
		    	combine texture + Constant
		    }
		}
	}
}
