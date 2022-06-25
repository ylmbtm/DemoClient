// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MyMobile/VertexLit/DirectLight" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
	  _LigthPos("LightPos",Vector) = (0,0,0,0)
	  _LigthColor("LigthColor",Color) = (0,0,0,0)
	  _Intensity("Intensity",Range(0,8)) = 1
    }

    SubShader {
       Tags {"Queue"="Geometry" "RenderType" = "Opaque" }
       LOD 20

	   Pass { 
          Lighting Off
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
