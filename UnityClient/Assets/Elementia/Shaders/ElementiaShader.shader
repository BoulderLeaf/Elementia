// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// colored vertex lighting
Shader "Elementia"
{
    // a single color property
    Properties {
		_DirtTex ("Dirt", 2D) = "defaulttexture" {}
		_GrassTex ("Grass", 2D) = "defaulttexture" {}
		_RockTex ("Rock", 2D) = "defaulttexture" {}
    }
    // define one subshader
    SubShader {
	Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      
     /* float _Grass01[1023];
            float _Grass02[1023];
            float _Grass03[1023];
            float _Grass04[1023];
            float _Grass05[1023];
            float _Grass06[1023];
            float _Grass07[1023];*/
      
      struct Input {
          float2 uv_DirtTex;
          float2 uv_GrassTex;
          float2 uv_RockTex;
      };
      sampler2D _DirtTex;
      sampler2D _GrassTex;
      sampler2D _RockTex;
      int _Grass01[1023];
      int _Grass02[1023];
      int _Grass03[1023];
      void surf (Input IN, inout SurfaceOutput o) {
      
      //get colors of textures
           half3 col1 = tex2D (_DirtTex, IN.uv_DirtTex).rgb;
           half3 col2 = tex2D (_GrassTex, IN.uv_GrassTex).rgb;
           half3 col3 = tex2D (_RockTex, IN.uv_RockTex).rgb;
           //blend them somehow - here I just use average color
           float finalR = col1.r/3 + col2.r/3 + col3.r/3;
           float finalG = col1.g/3 + col2.g/3 + col3.g/3;
           float finalB = col1.b/3 + col2.b/3 + col3.b/3;
           half3 final;
           final.r = finalR;
           final.g = finalG;
           final.b = finalB;    
           o.Albedo = col2; 
      
      
          //o.Albedo = tex2D (_DirtTex, IN.uv_DirtTex).rgb;
      }
      ENDCG
		/*Pass {
			Lighting Off

			 CGPROGRAM
			 
            float _Grass01[1023];
            float _Grass02[1023];
            float _Grass03[1023];
            float _Grass04[1023];
            float _Grass05[1023];
            float _Grass06[1023];
            float _Grass07[1023];
			 
            // use "vert" function as the vertex shader
            #pragma vertex vert
            // use "frag" function as the pixel (fragment) shader
            #pragma fragment frag
            #pragma surface surf Lambert

            // vertex shader inputs
           /* struct appdata
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            // vertex shader outputs ("vertex to fragment")
            struct v2f
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            // vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                // transform position to clip space
                // (multiply with model*view*projection matrix)
                o.vertex = UnityObjectToClipPos(v.vertex);
                // just pass the texture coordinate
                o.uv = v.uv;

                return o;
            }
            
            // texture we will sample
            sampler2D _DirtTex;

            // pixel shader; returns low precision ("fixed4" type)
            // color ("SV_Target" semantic)
            fixed4 frag (v2f i) : SV_Target
            {
                // sample texture and return it
                fixed4 col = tex2D(_DirtTex, i.uv);
                return col;
            }*/
            
            struct Input {
                  float2 uv_DirtTex;
              };
              
               sampler2D _DirtTex;
              void surf (Input IN, inout SurfaceOutput o) {
                  o.Albedo = tex2D (_DirtTex, IN.uv_DirtTex).rgb;
              }
            
            ENDCG
		}*/
	}
}