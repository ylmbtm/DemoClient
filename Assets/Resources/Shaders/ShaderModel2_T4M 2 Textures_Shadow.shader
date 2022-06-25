Shader "T4MShaders/ShaderModel2/Diffuse/T4M 2 Textures_Shadow" {
Properties {
	_Splat0 ("Layer 1", 2D) = "white" {}
	_Splat1 ("Layer 2", 2D) = "white" {}
	_Control ("Control (RGBA)", 2D) = "white" {}
	_MainTex ("Never Used", 2D) = "white" {}
	_ShadowTex("ShadowTex",2D) = "white" {}
	_ShadowColor("ShadowColor",Color) = (1,1,1,0.5)
}
                
SubShader {
	Tags {
   "SplatCount" = "2"
   "RenderType" = "Opaque"
	}
CGPROGRAM
#pragma surface surf Lambert 
#pragma exclude_renderers xbox360 ps3
struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float3 worldPos;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1;

uniform float4x4 _GlobalProjector;
uniform sampler2D _ShadowTex;
float4 _ShadowColor;
 
void surf (Input IN, inout SurfaceOutput o) {
	fixed2 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g);

	fixed4 uv = mul(_GlobalProjector,fixed4(IN.worldPos,1));
	fixed4 shadowTex = tex2Dproj(_ShadowTex,uv);
	shadowTex.a = (1 - shadowTex.a * _ShadowColor.a);
	shadowTex.rgb = _ShadowColor.rgb * shadowTex.a;

	o.Albedo.rgb *= shadowTex.rgb;
}
ENDCG 
}
}
