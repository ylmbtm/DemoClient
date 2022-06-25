Shader "MyMobile/Color/Blend" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType" = "Transparent" }

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