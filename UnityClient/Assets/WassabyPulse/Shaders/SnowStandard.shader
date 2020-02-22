Shader "Wassaby Pulse/Snow Standard" {
	Properties {
		_Color ("Base Color", Color) = (1,1,1,1)
		_SnowColor ("Snow Color", Color) = (1,1,1,1)
		_Metallic ("Metallic", Range(0,1)) = 0
		_Gloss ("Smoothness", Range(0,1)) = 0.5
		_OcclusionStrength ("Occlusion", Range(0,1)) = 1
		_Snow ("Snow Amount", Range(0,1)) = 0.5
		_SnowBlend ("Snow Blend", Range(0,1)) = 0.5
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset]_MetallicGlossMap("Metallic (Map)", 2D) = "white" {}
		[NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
		//[NoScaleOffset]_Emission ("Emission", 2D) = "black" {}
		[NoScaleOffset]_Occlusion("Occlusion", 2D) = "white" {}
		_SecondaryTex ("Snow (RGB)", 2D) = "white" {}
		[NoScaleOffset]_SecondaryBump ("Snow Normal", 2D) = "bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;
		//sampler2D _Emission;
		sampler2D _Occlusion;
		sampler2D _SecondaryTex;
		sampler2D _SecondaryBump;

		struct Input {
			float3 worldNormal;
			float2 uv_MainTex;
			float2 uv_SecondaryTex;
			
			INTERNAL_DATA
		};
		
		half _OcclusionStrength;
		half _Snow;
		half _SnowBlend;
		half _Metallic;
		half _Gloss;
		fixed4 _Color;
		fixed4 _SnowColor;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed3 worldUp = fixed3(0,1,0);
			fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			fixed3 snowNormal = UnpackNormal(tex2D(_SecondaryBump, IN.uv_SecondaryTex));
			fixed4 occ = lerp(tex2D (_Occlusion, IN.uv_MainTex), 1, 1 - _OcclusionStrength);
			//fixed4 em = tex2D (_Emission, IN.uv_MainTex);
			fixed4 def_albedo = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 snow_albedo = tex2D (_SecondaryTex, IN.uv_SecondaryTex) * _SnowColor;
			fixed4 metallic = tex2D (_MetallicGlossMap, IN.uv_MainTex);
			o.Normal = normal;
			
			float3 worldNormal = WorldNormalVector(IN, o.Normal);
			half rim = saturate(dot(worldNormal, normalize(worldUp)));
			half snowMap = smoothstep(rim*_SnowBlend, rim,1 - _Snow);

			o.Normal = lerp(snowNormal + normal, normal, snowMap);
			o.Albedo = lerp(snow_albedo, def_albedo ,snowMap)* occ;
			o.Metallic = metallic.rgb * _Metallic;
			o.Smoothness = metallic.a * _Gloss;
			//o.Emission = em;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
