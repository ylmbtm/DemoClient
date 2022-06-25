Shader "MyMobile/Transparent/Depht01" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CutBuf ("CutBuf",Range(0,.9)) = 0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 50
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		CGPROGRAM
		#pragma surface surf Lambert
		
		sampler2D _MainTex;
		half _CutBuf;
		
		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			if(c.a >= _CutBuf)
			   c.a = 1;
			else
			   c.a = 0;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
}
