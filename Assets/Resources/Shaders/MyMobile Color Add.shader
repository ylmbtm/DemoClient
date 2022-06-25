Shader "MyMobile/Color/Add" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType" = "Opaque" }
	    LOD 50

	    Blend SrcAlpha One
		ZWrite off
		Lighting On
		Pass { 
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