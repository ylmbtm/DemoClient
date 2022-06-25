Shader "MyMobile/Illumin/Diffuse" {
Properties {
	_Alpha ("Alpha", Range(0,1)) = 1
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}
SubShader {
	Tags { "RenderType"="Opaque"}
	LOD 50
    Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed _Alpha;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);// * _Alpha;
	tex.a *= _Alpha;
	o.Albedo = tex.rgb;
	o.Emission = tex.rgb;
	o.Alpha = tex.a;
}
ENDCG
} 

}
