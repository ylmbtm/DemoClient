// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Camera/ScreenDepthOfField"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog {Mode off}
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

		    struct v2f
	        {
		        float4 pos : POSITION;
		        float2 uv : TEXCOORD0;
	        };

		    // 摄像机所见纹理  
		    sampler2D _MainTex;
		    // 摄像机深度纹理  
		    sampler2D _CameraDepthTexture;
		    // 模糊纹理  
		    sampler2D _BlurTex;

		    v2f vert(appdata_img v)
		    {
			    v2f o;
		    	o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv.xy = v.texcoord.xy;
			    return o;
		    }

		    half4 frag(v2f i) : COLOR
		    {
			    // 摄像机纹理  
			    half4 ori = tex2D(_MainTex,i.uv);
			    // 模糊纹理  
			    half4 blur = tex2D(_BlurTex,i.uv);
			    // 获取摄像机深度值颜色值，取rgb都可以  
			    float dep = tex2D(_CameraDepthTexture,i.uv).r;
			    // 深度Z缓存，从摄像机到最远平截面[0,1]  
			    dep = Linear01Depth(dep);
			    // (1-dep) * ori + dep * blur  
			    // 所以靠近摄像机的不模糊，越远越模糊  
			    return lerp(ori,blur,dep);
		     }
		    ENDCG
	    }
	}
}
