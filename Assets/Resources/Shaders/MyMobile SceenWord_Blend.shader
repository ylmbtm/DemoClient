// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyMobile/SceenWord_Blend" {
	Properties {
		_Color("Color",Color) = (1,1,1,1)
		_WordTex1 ("WordTex1", 2D) = "white" {}
		_WordTex2 ("WordTex2", 2D) = "white" {}
		_WordTex3 ("WordTex3", 2D) = "white" {}
		_WordTex4 ("WordTex4", 2D) = "white" {}
		_WordTex5 ("WordTex5", 2D) = "white" {}
		_WordCount ("WorldCount",float) = 1
	}
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Opaque" }
		LOD 50
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off
		ZTest Always
		pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma exclude_renderers flash

			sampler2D _WordTex1;
			sampler2D _WordTex2;
			sampler2D _WordTex3;
			sampler2D _WordTex4;
			sampler2D _WordTex5;
			
			float _WordCount;
			half4 _Color;
			
			struct v2f{
				float4 pos : SV_POSITION;
				float2 uvs[5] : TEXCOORD1;
			};

			struct input_date {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD;
				
			};

			v2f vert(input_date v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				int count = _WordCount;
				for(int i = 0;i < 5;i++)
				{
					o.uvs[i].x = v.texcoord.x * (float)count - (float)i;
					o.uvs[i].y = v.texcoord.y;
				}
				return o;
			}
			
			half4 frag(v2f i):COLOR
			{
				half4 words[5];
				words[0] = tex2D(_WordTex1,i.uvs[0]);
				words[1] = tex2D(_WordTex2,i.uvs[1]);
				words[2] = tex2D(_WordTex3,i.uvs[2]);
				words[3] = tex2D(_WordTex4,i.uvs[3]);
				words[4] = tex2D(_WordTex5,i.uvs[4]);

				half4 word = fixed4(0,0,0,0);
				for(int i = 0;i < 5;i++)
				{
					if(_WordCount <= i)
						break;	
					word += words[i] * words[i].a;
				}
				
				if(word.a < 0.7)
					word.a = 0;
				else
					word.a = 1;
					
				word.rgb *= word.a;
				word.a *= _Color.a;

				return word;
			}
			ENDCG
		}
	} 
}
