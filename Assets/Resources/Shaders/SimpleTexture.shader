Shader "Simple Texture" {
   Properties {
      _Color ("Main Color", Color) = (1, 1, 1, 1)
      _MainTex ("Base (RGB)", 2D) = "white"
   }
   SubShader {
      Pass {
         SetTexture [_MainTex] {
            constantColor [_Color]
            Combine texture * constant
         }
      }
   }
}