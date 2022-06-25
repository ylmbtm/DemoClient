Shader "MyMobile/Color/Blend_Late" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent+100" "RenderType" = "Opaque" }

	    Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		Lighting On
		
		Pass { 
		    Name "COLORBLEND"
			Material {
                Diffuse [_Color]
                Ambient [_Color]
                Shininess [_Color]
                Specular [_Color]
                Emission [_Color]
            }
       }
	} 
}