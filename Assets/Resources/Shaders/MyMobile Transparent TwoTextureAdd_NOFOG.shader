Shader "MyMobile/Transparent/TwoTextureAdd_NOFOG" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_MoonGlow("Glow",2D) = "black"{}
	_GlowColor("Color",Color) = (0.5,0.5,0.5,0.5)
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 50
	fog{mode off}
    CGPROGRAM
    #pragma surface surf Lambert alpha

    sampler2D _MainTex;
    sampler2D _MoonGlow;
	half4 _GlowColor;

    struct Input {
    	float2 uv_MainTex;
    	float2 uv_MoonGlow;
    };

    void surf (Input IN, inout SurfaceOutput o) {
    	half4 c = tex2D(_MainTex, IN.uv_MainTex);
    	half4 m = tex2D(_MoonGlow, IN.uv_MoonGlow);
    	m.a = (m.r + m.g + m.b) / 3;
    	m *= _GlowColor;
    	half4 ot = m * m.a + c;
    	o.Albedo = ot.rgb;
    	o.Alpha = ot.a;
    }
    ENDCG
    }
}
