// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MMO Hero"
{
	Properties
	{
		_MainColor("Main Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_MainTex("Main Color Texture", 2D) = "white" {}
     	_Wrap("Light Wrap", float) = 0.25
		_NormalMap("Normal Map", 2D) = "bump" {}
	    _BumpDepth("Bump Depth", Range(0.1, 4.0)) = 1
	}

	SubShader
	{
		Pass
	    {
		   Tags{ "LightMode" = "ForwardBase" }
		   CGPROGRAM

           #pragma vertex vert
           #pragma fragment frag
           #pragma target 3.0
           #pragma multi_compile_fwdadd_fullshadows
           #include "UnityCG.cginc"
           #include "AutoLight.cginc"

		   //user defined
		   uniform sampler2D _MainTex;
	       uniform sampler2D _NormalMap;

	       uniform float4	_MainTex_ST;
	       uniform float4	_NormalMap_ST;
	       uniform float4 	_MainColor;

	       uniform float	_Wrap;
	       uniform float   _BumpDepth;

	       //unity defined
	       uniform float4 	_LightColor0;

	       //base input struct
	       struct vertexInput
	       {
	        	float4 vertex : POSITION;
	        	float3 normal : NORMAL;
		        float4 texcoord : TEXCOORD0;
		        float4 tangent : TANGENT;
	       };

	       struct vertexOutput
	       {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalWorld : TEXCOORD2;
				float3 tangentWorld : TEXCOORD3;
				float3 binormalWorld : TEXCOORD4;

				LIGHTING_COORDS(5,6)
			};

	//vertex function
		   vertexOutput vert(vertexInput v)
		   {
		vertexOutput o;

		float4x4 modelMatrix = unity_ObjectToWorld;
		float4x4 modelMatrixInverse = unity_WorldToObject;

		o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
		o.tangentWorld = normalize(mul(unity_ObjectToWorld, half4(half3(v.tangent.xyz), 0)));
		o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);

		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		o.pos = UnityObjectToClipPos(v.vertex);
		o.tex = v.texcoord;

		TRANSFER_VERTEX_TO_FRAGMENT(o); // for shadows

		return o;

	}

	//take a -1 to 1 range and fit it 0 to 1
	float clamp01(float toBeNormalized)
	{
		return toBeNormalized * 0.5 + 0.5;
	}

	float3 calculateAmbientReflection(float3 rsrm , float texM)
	{
		float  mask = (rsrm.x + rsrm.y + rsrm.z) * 0.33;
		float3 amb = UNITY_LIGHTMODEL_AMBIENT.xyz;
		return  float3 (1.5 * rsrm * amb + amb * 0.5 * texM);
	}

	//fragment function
	float4 frag(vertexOutput i) : COLOR
	{
		float shadAtten = LIGHT_ATTENUATION(i);

	float4 tex = tex2D(_MainTex,   i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
	//tex  = tex  * _MainColor;
	float4 texN = tex2D(_NormalMap, i.tex.xy * _NormalMap_ST.xy + _NormalMap_ST.zw);
	float nDepth = 8 / (_BumpDepth * 8);

	//Unpack Normal
	half3 localCoords = half3(2.0 * texN.ag - float2(1.0, 1.0), 0.0);
	localCoords.z = nDepth;

	//normal transpose matrix
	float3x3 local2WorldTranspose = float3x3
		(
			i.tangentWorld,
			i.binormalWorld,
			i.normalWorld
			);

	//Calculate normal direction
	float3 normalDir = normalize(mul(localCoords, local2WorldTranspose));

	float3 N = normalize(normalDir);
	float3 V = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float3 fragmentToLight = _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
	float  distanceToLight = length(fragmentToLight);
	float  atten = pow(2, -0.1 * distanceToLight * distanceToLight) * _WorldSpaceLightPos0.w + 1 - _WorldSpaceLightPos0.w; // (-0.1x^2)^2 for pointlights 1 for dirlights
	float3 L = (normalize(fragmentToLight)) * _WorldSpaceLightPos0.w + normalize(_WorldSpaceLightPos0.xyz) * (1 - _WorldSpaceLightPos0.w);
	float3 H = normalize(V + L);
	float3 worldReflect = reflect(V,N);

	//lighting
	float NdotL = dot(N,L);
	float NdotV = 1 - max(0.0, dot(N,V));
	float NdotH = clamp(dot(N,H), 0, 1);
	float VdotL = clamp01(dot(V,L));
	float wrap = clamp(_Wrap, -0.25, 1.0);

	float4 texdesat = dot(tex.rgb, float3(0.3, 0.59, 0.11));

	float3 difftex = tex.xyz;

	VdotL = pow(VdotL, 0.85);
	float bellclamp = (1 / (1 + pow(0.65 * acos(dot(N,L)), 16)));

	float3 spec = NdotH;
	spec = pow(spec, 32.0);

	float3 diff = max(0, (pow(max(0, (NdotL * (1 - wrap) + wrap)), (2 * wrap + 1))));
	diff *= lerp(shadAtten, 1, wrap) * atten * difftex.xyz * _LightColor0.rgb;

	return float4 (atan(clamp(spec + diff, 0, 2)), 1); //this is used to round off values above one and give better color reproduction in bright scenes					
	}

		ENDCG
	}
	}
		Fallback "Diffuse"
}