// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "MyMobile/Water/HighMap" {
	Properties {
		_WaterTex ("Normal Map (RGB), Foam (A)", 2D) = "white" {}
		_High("High Map",2D) = "black" {}
		_Color0 ("Shallow Color", Color) = (1,1,1,1)
		_Color1 ("Deep Color", Color) = (0,0,0,0)
		_Specular ("Specular", Color) = (0,0,0,0)
		_Tiling ("Tiling", Range(0.025, 0.25)) = 0.25
		_Pass("RGBPass",Vector) = (1,0,0,0)
		_LightPos("LightPos",Vector) = (1,-1,0,0)
	}
	
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Lighting Off
		pass
		{
			Tags{"LightMode"="ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma exclude_renderers flash
			#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
			
			half4 _Color0;
			half4 _Color1;
			half4 _Specular;
			half _Tiling;
			sampler2D _WaterTex;
			sampler2D _High;
			half4 _High_ST;
			half4 _Pass;
			half4 _LightPos;
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float3 wPos : TEXCOORD1;
				float3 wView : TEXCOORD2;
				float4 tilUV : TEXCOORD3;
			};
			
			struct app_date{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};
			
			struct Input{
				float3 Albedo;
				float3 Normal;
				float3 Emission;
				float Alpha;
			};
			
			half4 LightingPPL (Input s, half3 lightDir, half3 viewDir)
			{
				half3 nNormal = normalize(s.Normal);

				half diffuseFactor = max(0.0, dot(nNormal, lightDir));
				half specularFactor = max(0.0, dot(viewDir, reflect(lightDir, nNormal))) * s.Alpha;
				half4 c;
				c.rgb = s.Albedo * diffuseFactor + _Specular.rgb * specularFactor * s.Albedo + s.Emission;
				c.a = s.Alpha;
				return c;
			}
			
			v2f vert(app_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv = v.texcoord0;
			    o.wPos = mul(unity_ObjectToWorld,v.vertex).xyz;
			    
			    o.wView = _WorldSpaceCameraPos - o.wPos;

				float offset = _Time.x * 0.5;
				float2 tiling = o.wPos.xz * _Tiling;

				o.tilUV.xy = tiling + offset;
				o.tilUV.zw = float2(-tiling.y, tiling.x) - offset;

				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
			    Input IN;
				half2 uv = TRANSFORM_TEX(i.uv,_High);
				half4 dephtCol = tex2D(_High,uv) * _Pass;
				half depth = dephtCol.r + dephtCol.g + dephtCol.b + dephtCol.a;
				depth = 1 - depth;
	
				half4 normalMap = (tex2D(_WaterTex, i.tilUV.xy) + tex2D(_WaterTex, i.tilUV.zw)) * 0.5;
				IN.Normal = normalMap.rgb * 2 - 1;
			
				half4 col;
				half3 col0 = _Color0.rgb * _Color0.a * 3;
				half3 col1 = _Color1.rgb * _Color1.a * 3;
				col.rgb = lerp(col0.rgb,col1.rgb,depth);
				col.a = depth;
				col.rgb *= col.rgb;
				half foam = normalMap.a * (1.0 - abs(col.a * 2.0 - 1.0)) * 0.35;
				
				half fresnel = dot(normalize(i.wView), IN.Normal);
				fresnel *= fresnel;
				fresnel = 1 - fresnel;
				IN.Alpha = col.a;
				IN.Albedo = col.rgb * depth + foam;
				IN.Emission = IN.Albedo * fresnel;
				
				//half3 lightDir = _WorldSpaceLightPos0.xyz;
				half3 lightDir = _LightPos.xyz;
				half4 outColor = LightingPPL(IN,normalize(lightDir),normalize(i.wView));
				
				return outColor;
			}		
			ENDCG
		}
	} 
}
