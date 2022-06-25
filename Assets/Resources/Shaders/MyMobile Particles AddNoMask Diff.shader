Shader "MyMobile/Particles/AddNoMask-Diff" {
	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
		_Alpha("Alpha",Color) = (0.5,0.5,0.5,0.5)
		_Color ("Color", Color) = (1,1,1,1)
		_AlphaCon("AlphaCon",Range(0,1)) = 1
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }	
		Blend SrcAlpha One
	Cull Off 
	Lighting Off 
	ZWrite Off 
	Fog { Color (0,0,0,0) }
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
				Pass {
		    Name "ADD"
			SetTexture [_MainTex] {
			    ConstantColor[_Alpha]
				combine texture * primary,texture * Constant
			}
			
			SetTexture[_MainTex]{
			    ConstantColor[_Color]
				combine Previous * Constant
			}
			
			SetTexture[_MainTex]{
			    ConstantColor(1,1,1,[_AlphaCon])
			    combine Previous * Constant
			}
		}
	}
}
