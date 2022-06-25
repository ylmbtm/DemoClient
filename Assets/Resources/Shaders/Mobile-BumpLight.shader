// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Self/NPCshader (Supports Backlight, Light Probes)"
{
    Properties {
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "grey" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _BRDFTex ("NdotL NdotH (RGBA)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
        LOD 100

        CGPROGRAM
#pragma surface surf PseudoBRDF exclude_path:prepass vertex:separateSH nolightmap noforwardadd noambient approxview

        struct MySurfaceOutput {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            half Specular;
            fixed Gloss;
            fixed Alpha;
            fixed3 OcclusionAndAmbientLight;
        };

        sampler2D _BRDFTex;

        inline fixed4 LightingPseudoBRDF (MySurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
        {
            // Half vector
            fixed3 halfDir = normalize (lightDir + viewDir);

            // N.L
            fixed NdotL = dot (s.Normal, lightDir);
            // N.H
            fixed NdotH = dot (s.Normal, halfDir);

            // remap N.L from [-1..1] to [0..1]
            // this way we can shade pixels facing away from the light - helps to simulate bounce lights
            fixed biasNdotL = NdotL * 0.5 + 0.5;

            // lookup light texture
            //  rgb = diffuse term
            //    a = specular term
            fixed4 l = tex2D (_BRDFTex, fixed2(biasNdotL, NdotH));

            fixed4 c;
            // mask specular term by Gloss factor
            // modulate specular with Albedo to allow metalic-ish look
            //
            // Shadowgun style: instead of adding LightProbes, treat them as both occlusion for MainLight and Ambient bounce
            // that is not physically correct, but it
            // 1) provides way to occlude MainLight without using runtime shadows
            // 2) allows bounce light to be affected by per-pixel normals
            // note that bounce lights becomes much weaker!
            c.rgb = s.OcclusionAndAmbientLight * s.Albedo * (l.rgb + s.Gloss * l.a) * 2;
            c.a = 0;

            return c;
        }


        sampler2D _MainTex;
        sampler2D _BumpMap;


        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            fixed3 shOcclusionAndAmbient;
        };

        void separateSH (inout appdata_full v, out Input o)
        {
            o.uv_MainTex = 0;
            o.uv_BumpMap = 0;
            o.shOcclusionAndAmbient = 0;
            float3 worldN = mul ((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
            o.shOcclusionAndAmbient = ShadeSH9 (float4(worldN, 1.0));
        }

        void surf (Input IN, inout MySurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb;
            o.Gloss = tex.a * 0.5;
            o.Alpha = tex.a;
//  o.Normal = tex2D(_BumpMap, IN.uv_BumpMap).rgb * 2.0 - 1.0;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.OcclusionAndAmbientLight = 0.3 + IN.shOcclusionAndAmbient * 0.4;
        }
        ENDCG

    }

    FallBack "Mobile/Diffuse"
}
