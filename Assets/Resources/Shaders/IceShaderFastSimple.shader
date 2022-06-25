Shader "Effects/Ice/IceFront" {
Properties {
      _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_ReflectionStrength ("ReflectionStrength", Range (1, 20)) = 1
        _MainTex ("Base (RGB) Emission Tex (A)", 2D) = "white" {}
		_Opacity ("Material opacity", Range (-1, 1)) = 0.5
        _Cube ("Reflection Cubemap", Cube) = "" { TexGen CubeReflect }
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
		_Cutoff ("Cutoff", Range (0, 1)) = 0.5
		_LightStr ("Light strength", Range (0, 1)) = 1
}

SubShader {
        Tags { "Queue"="Transparent+1" "RenderType"="Transperent" }
        LOD 200
		
CGPROGRAM
#pragma surface surf Lambert alpha 
#pragma target 3.0
#pragma glsl

sampler2D _MainTex;
sampler2D _BumpMap;
samplerCUBE _Cube;

float4 _Color;
float4 _ReflectColor;
float _ReflectionStrength;
float _Shininess;
float _FPOW;
float _R0;
float _Opacity;
float _Cutoff;
float _LightStr;

struct Input {
        float2 uv_MainTex;
        float2 uv_BumpMap;
        float3 viewDir;
        float3 worldRefl;
        INTERNAL_DATA
};
 
void surf (Input IN, inout SurfaceOutput o) {
        half4 tex = tex2D(_MainTex, IN.uv_MainTex);
        half4 c = tex * _Color;
        o.Albedo = _Color;
       
        o.Gloss = tex.a;
        o.Specular = _Shininess;
       
        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
       
        float3 worldRefl = WorldReflectionVector (IN, o.Normal);

        half4 reflcol = texCUBE (_Cube, worldRefl);
        reflcol *= tex.a;     

        half fresnel = saturate(1.0 - dot(o.Normal, normalize(IN.viewDir)));
        fresnel = pow(fresnel, _FPOW);
        fresnel = _R0 + (1.0 - _R0) * fresnel;
        reflcol = lerp(c, reflcol, fresnel);

        o.Emission = reflcol.rgb * _ReflectColor.rgb * _ReflectionStrength * _LightStr;
        o.Alpha = _Cutoff > tex.a ? tex.a+_Opacity : 0;
}
ENDCG
}

FallBack "Reflective/Bumped Diffuse"
}