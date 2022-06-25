Shader "MyMobile/Color/Diffuse" {
	Properties {
		//_Shininess("Shininess",Range(0.01,1)) = 0.7
		_Color ("Main Color", Color) = (1,1,1,1)
		//_SpecColor("Spec Color", Color) = (1,1,1,1)
		_Emission("Emmisive Color", Color) = (0,0,0,0)
	}
	SubShader {
		Tags {"Queue"="Geometry" "RenderType" = "Opaque" }

		Lighting On
		//SeparateSpecular On
		Pass { 
			Material {
                Diffuse [_Color]
                //Ambient [_Color]
                //Shininess [_Shininess]
                //Specular [_SpecColor]
                Emission [_Emission]
            }
       }
	} 
}
