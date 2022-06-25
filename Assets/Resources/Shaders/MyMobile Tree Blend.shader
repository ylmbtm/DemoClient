Shader "MyMobile/Tree/Blend" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Alpha ("Alpha",Range(0,1)) = 0
		_Pow("Pow",Range(1,20)) = 1
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		CGPROGRAM
		#pragma surface surf Lambert
		
		sampler2D _MainTex;
		fixed _Alpha;
		fixed _Pow;
		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			if(c.a >= _Alpha)
			   c.a = 1;
			else
			   c.a = saturate(1-(1.5 * _Alpha - c.a));
			c.a = pow(c.a,_Pow);
			c = c * c.a;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
