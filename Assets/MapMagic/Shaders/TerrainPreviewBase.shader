Shader "MapMagic/TerrainPreviewBase" 
{
	Properties
	{
		[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
		_Preview("Control (RGBA)", 2D) = "red" {}
		_Whites("Whites", Color) = (0,1,0,1)
		_Blacks("Blacks", Color) = (1,0,0,1)
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer fullforwardshadows
		#pragma multi_compile_fog
		#pragma target 3.0
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _TERRAIN_NORMAL_MAP

		#define TERRAIN_STANDARD_SHADER
		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
		#include "TerrainSplatmapCommon.cginc"


		sampler2D _Preview;
		half4 _Whites;
		half4 _Blacks;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			//half4 splat_control;
			//half weight;
			//fixed4 mixedDiffuse;
			//half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
			//SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
			//o.Albedo = mixedDiffuse.rgb;
			//o.Alpha = weight;
			//o.Smoothness = mixedDiffuse.a;
			//o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));

			half4 color = tex2D(_Preview, IN.tc_Control);
			float val = (color.r+color.g+color.b)/3;
			o.Albedo = _Whites*val + _Blacks*(1-val);
			o.Alpha = 1;
		}
		ENDCG
	}

	Fallback "Nature/Terrain/Diffuse"
}