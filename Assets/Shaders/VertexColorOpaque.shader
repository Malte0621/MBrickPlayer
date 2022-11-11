 Shader "Custom/VertexColorOpaque" {
     Properties {
         _Color ("Color", Color) = (1,1,1,1)
         _MainTex ("Albedo (RGB)", 2D) = "white" {}
         _NormalTex ("Normal", 2D) = "bump" {}
         //_BumpTex ("Bump", 2D) = "white" {}
         _Glossiness ("Smoothness", Range(0,1)) = 0.5
         _Metallic ("Metallic", Range(0,1)) = 0.0
         //_Parallax("Parallax", float) = 0
     }
     SubShader {
         Tags { "RenderType"="Opaque"}
         LOD 200
         
         CGPROGRAM
         #pragma surface surf Standard vertex:vert fullforwardshadows
         #pragma target 3.0
         struct Input {
             float2 uv_MainTex;
             float2 uv_NormalTex;
             //float2 uv_BumpTex;
             float4 vertexColor; // Vertex color stored here by vert() method
             float3 viewDir;
         };
         
         struct v2f {
           float4 pos : SV_POSITION;
           float4 color : COLOR;
         };
 
         void vert (inout appdata_full v, out Input o)
         {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
         }
 
         sampler2D _MainTex;
         sampler2D _NormalTex;
         //sampler2D _BumpTex;
 
         half _Glossiness;
         half _Metallic;
         fixed4 _Color;
         //float _Parallax;
 
         void surf (Input IN, inout SurfaceOutputStandard o) 
         {
            // calculate bump
            //float bumpTex = tex2D(_BumpTex, IN.uv_BumpTex).r;
            //float2 parallaxOffset = ParallaxOffset(bumpTex, _Parallax, IN.viewDir);

            // do regular things now
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
            o.Albedo = c.rgb * IN.vertexColor; // Combine normal color with the vertex color

            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
         }
         ENDCG
     } 
     FallBack "Diffuse"
 }
 