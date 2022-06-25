// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/XRayAndBackLightVertex" {
    Properties {
	  _HitColor("HitColor",Color) = (0,0,0,0)
      _MainTex ("Texture", 2D) = "white" {}
	  _BlendColor ("XRayLineColor", Color) = (1,1,1,1)
	  _BlendVal("XRayLineRange(0.5-10)",Range(0.5,100)) = 3.0
	  _BlendPower("XRayLinePower(1-20)",Range(1,20)) = 1
    }

	
    SubShader {
       Tags {"Queue"="Geometry-100" "RenderType" = "Opaque" }
	   LOD 100
	   Pass { 
          Blend SrcAlpha OneMinusSrcAlpha 
          Cull Back 
          Lighting Off 
          ZWrite Off 
		  ZTest Greater

		  CGPROGRAM
		  #pragma vertex vert
          #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest
          #include "UnityCG.cginc"

          struct v2f {
             float4 pos : SV_POSITION;
			 float2 uv : TEXCOORD0;
			 fixed rim : TEXCOORD1;
          };

		  struct my_date{
		      float4 vertex : POSITION;
			  float2 texcoord0 : TEXCOORD0;
			  float3 normal : NORMAL;
		  };

          sampler2D _MainTex;
		  half4 _BlendColor;
		  half _BlendVal;
		  half _BlendPower;
		  half4 _HitColor;

		  v2f vert (my_date v)
         {
		    half3 view = ObjSpaceViewDir(v.vertex);
			half rim = 1.0 - saturate(dot (normalize(view), v.normal));
            v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord0;
			o.rim = rim;
            return o;
         }

		 half4 frag (v2f i) : COLOR
         {
			fixed4 blcolor = _BlendColor * pow (i.rim, _BlendVal) * _BlendPower;
            float4 texCol = _HitColor;
	        float4 outp = texCol + blcolor;
	        return outp;
	     }
         ENDCG
       } 


       Tags {"Queue"="Geometry+100" "RenderType" = "Opaque" }
	   Pass { 
          ZWrite On 
	      ZTest LEqual 
          Lighting On
		  
		  CGPROGRAM
		  #pragma vertex vert
          #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest

		  struct v2f {
             float4 pos : SV_POSITION;
			 float2 uv : TEXCOORD0;
          };

		  struct my_date{
		      float4 vertex : POSITION;
			  float2 texcoord0 : TEXCOORD0;
		  };

		  sampler2D _MainTex;
		  half4 _HitColor;

		  v2f vert (my_date v)
         {
            v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord0;
            return o;
         }

		 float4 frag (v2f i) : COLOR
         {
            float4 texCol = tex2D(_MainTex,i.uv) + _HitColor;
	        return texCol;
	     }
         ENDCG
       }
    } 
}