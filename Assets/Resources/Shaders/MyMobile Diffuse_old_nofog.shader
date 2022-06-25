Shader "MyMobile/Diffuse_old_NOFOG" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Geometry" }
		LOD 50
	FOG{Mode OFF}
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
		}
	Fallback "Mobile/VertexLit"
}


