// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MyMobile/XRayAndBackLightVertex" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
	  _BlendColor ("XRayLineColor", Color) = (1,1,1,1)
	  _BlendVal("XRayLineRange(0-1)",Range(0,1)) = 0
	  //_BlendPower("XRayLinePower(1-20)",Range(1,20)) = 1
	  _LigthPos("LightPos",Vector) = (0,0,0,0)
	  _LigthColor("LigthColor",Color) = (0,0,0,0)
	  _Intensity("Intensity",Range(0,8)) = 1
    }

    SubShader {
       Tags {"Queue"="Geometry" "RenderType" = "Opaque" }
       LOD 20
	   Pass { 
          Blend SrcAlpha OneMinusSrcAlpha 
          Cull Back 
          Lighting Off 
          ZWrite Off 
		  ZTest GEqual
		  
		  CGPROGRAM
		  #pragma vertex vert
          #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest
          #include "UnityCG.cginc"

          struct v2f {
             float4 pos : SV_POSITION;
			 fixed rim : TEXCOORD1;
          };

		  struct my_date{
		      float4 vertex : POSITION;
			  float3 normal : NORMAL;
		  };

		  half4 _BlendColor;
		  half _BlendVal;
		  //half _BlendPower;

		  v2f vert (my_date v)
         {
		    half3 view = ObjSpaceViewDir(v.vertex);
			half rim = 1.0 - saturate(dot(normalize(view), normalize(v.normal)));
            v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.rim = rim;
            return o;
         }

		 half4 frag (v2f i) : COLOR
         {
			fixed4 blcolor;
			if(i.rim > _BlendVal)
				blcolor = _BlendColor; //* pow (i.rim, _BlendVal) * _BlendPower;
			else
				blcolor = half4(0,0,0,0);
	        return blcolor;
	     }
         ENDCG
       } 


	   Pass { 
          ZWrite On 
	      ZTest Less 
          Lighting On
		  
		  CGPROGRAM
		  #pragma vertex vert
          #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest

		  struct v2f {
             float4 pos : SV_POSITION;
			 float2 uv : TEXCOORD0;
			 float4 color : TEXCOORD1;
          };

		  struct my_date{
		      float4 vertex : POSITION;
			  float2 texcoord0 : TEXCOORD0;
			  float3 normal : NORMAL;
		  };

		  sampler2D _MainTex;
		  float4 _LigthPos;
		  half4 _LigthColor;
		  half _Intensity;
		  
		  v2f vert (my_date v)
         {
            v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord0;
            
            float4 LigthPos = _LigthPos;
            LigthPos.w = 0;
            float4 ligthDirW = normalize(LigthPos);
            float4 ligthDirO = mul(ligthDirW,unity_ObjectToWorld);
            o.color = saturate(dot(ligthDirO.xyz,v.normal)) * _LigthColor * _Intensity;
            return o;
         }

		 float4 frag (v2f i) : COLOR
         {
            float4 texCol = tex2D(_MainTex,i.uv);
            texCol.rgb = texCol.rgb + texCol.rgb * i.color.rgb * texCol.a;
	        return texCol;
	     }
         ENDCG
       }
    } 
}