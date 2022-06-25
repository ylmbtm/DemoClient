Shader "Self/RumpSimple" { 
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
    _RimPower ("Rim Power", Range(0.0,2.0)) = 0.3
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 100
	cull off
		
CGPROGRAM
#pragma target 2.0
#pragma surface surf BlinnPhong addshadow

sampler2D _MainTex;
sampler2D _BumpMap;

fixed4 _Color;
float4 _RimColor;
float _RimPower;


struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float3 viewDir;
};

void vert (inout appdata_full v, out Input o) {}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb ;	
	o.Alpha = tex.a * _Color.a;	
	o.Gloss = tex.a*0.5;
//	o.Normal = tex2D(_BumpMap, IN.uv_BumpMap).rgb * 2.0 - 1.0;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	half rim = 1 - saturate(dot (normalize(IN.viewDir), o.Normal));
    o.Emission = _RimColor.rgb *rim*rim* _RimPower;
}
ENDCG
}

FallBack "Specular"
}
