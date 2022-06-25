Shader "MyMobile/Transparent/Depht" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 50
	Blend SrcAlpha OneMinusSrcAlpha
	Cull off
    CGPROGRAM
    #pragma surface surf Lambert

    sampler2D _MainTex;

    struct Input {
    	float2 uv_MainTex;
    };

    void surf (Input IN, inout SurfaceOutput o) {
    	half4 c = tex2D(_MainTex, IN.uv_MainTex);
    	o.Albedo = c.rgb;
    	o.Alpha = c.a;
    }
    ENDCG
    }
}
