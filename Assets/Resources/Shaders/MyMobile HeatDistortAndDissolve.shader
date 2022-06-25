// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/HeatDistortAndDissolve" {
Properties {
 _BumpAmt  ("BumpAmt", range (0,100)) = 10
 _MainTex ("Tint Color (RGB)", 2D) = "white" {}
 _BumpMap ("Normalmap", 2D) = "bump" {}
 _Dissolve ("Dissolve",Range(-0.01,1)) = -0.01
 _DissolveSpeed ("DissolveSpeed",Range(1,5)) = 1
 
 _Color ("Color",Color) = (0,0,0,0)
}

CGINCLUDE
#pragma fragmentoption ARB_precision_hint_fastest
#pragma fragmentoption ARB_fog_exp2
#include "UnityCG.cginc"

sampler2D _GrabTexture : register(s0);
float4 _GrabTexture_TexelSize;
sampler2D _BumpMap : register(s1);
sampler2D _MainTex : register(s2);
float4 _Color;

float _Dissolve;
float _DissolveSpeed;

struct v2f {
 float4 vertex : POSITION;
 float4 uvgrab : TEXCOORD0;
 float2 uvbump : TEXCOORD1;
 float2 uvmain : TEXCOORD2;
};

uniform float _BumpAmt;


half4 frag( v2f i ) : COLOR
{
 half4 mask = tex2D( _BumpMap, i.uvbump - _Time.x);
 half2 bump = UnpackNormal(mask).rg; 
 float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
 
 half4 tint = tex2D( _MainTex, i.uvmain) + _Color;
 half4 heatDisMask = tint;
 if(mask.a <= _Dissolve)
 {
     half alpha = saturate(tint.a - _Dissolve * 2 + mask.a);
     alpha = pow(alpha,_DissolveSpeed);
     tint.a = alpha;
 }
 
 i.uvgrab.xy = offset * i.uvgrab.z * heatDisMask.a + i.uvgrab.xy;
 half4 col = tex2Dproj(_GrabTexture, i.uvgrab);
 
 half4 outcol = lerp(col,tint,tint.a);
 
 return outcol;
}
ENDCG

Category {
 Tags { "Queue"="Transparent+20" "RenderType"="Opaque" }
 SubShader {
  GrabPass {       
   Name "BASE"
   Tags { "LightMode" = "Always" }
   }

  Blend SrcAlpha OneMinusSrcAlpha
  cull off
  
  Pass {
   Name "BASE"
   Tags { "LightMode" = "Always" }
   
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

struct appdata_t {
 float4 vertex : POSITION;
 float2 texcoord: TEXCOORD0;
 float2 texcoord1 : TEXCOORD1;
};

v2f vert (appdata_t v)
{
 v2f o;
 o.vertex = UnityObjectToClipPos(v.vertex);
 #if UNITY_UV_STARTS_AT_TOP
 float scale = -1.0;
 #else
 float scale = 1.0;
 #endif

 o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
 o.uvgrab.zw = o.vertex.zw;
 COMPUTE_EYEDEPTH(o.uvgrab.z);
 o.uvbump = MultiplyUV( UNITY_MATRIX_TEXTURE1, v.texcoord );
 o.uvmain = MultiplyUV( UNITY_MATRIX_TEXTURE2, v.texcoord );
 //o.uvbump = v.texcoord;
 //o.uvmain = v.texcoord;
 return o;
}
ENDCG
  }
 }
 
 SubShader {
  Blend DstColor Zero
  Pass {
   Name "BASE"
   SetTexture [_MainTex] { combine texture }
  }
 }
}

}
