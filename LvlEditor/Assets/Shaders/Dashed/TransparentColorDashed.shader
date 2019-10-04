// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader.
 // - no lighting
 // - no lightmap support
 // - no per-material color
 
 Shader "Unlit/Transparent Color Dashed" {
 Properties {
	 _Color("Main Color", Color) = (1,1,1,1)
     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
 }
 
 SubShader {
     Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
     LOD 100
     
     ZWrite Off
     Blend SrcAlpha OneMinusSrcAlpha 

     Pass {  
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fog
             
             #include "UnityCG.cginc"
 
             struct appdata_t {
                 float4 vertex : POSITION;
                 float2 texcoord : TEXCOORD0;
             };
 
             struct v2f {
                 float4 vertex : SV_POSITION;
                 half2 texcoord : TEXCOORD0;
                 UNITY_FOG_COORDS(1)
				 float4 worldSpacePos : TEXCOORD2;
             };
 
             sampler2D _MainTex;
			 float4 _Color;
             float4 _MainTex_ST;
             
             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                 UNITY_TRANSFER_FOG(o,o.vertex);
				 o.worldSpacePos = mul(unity_ObjectToWorld, v.vertex);
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {
                 fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                 UNITY_APPLY_FOG(i.fogCoord, col);

				 bool sliceX = (round(i.worldSpacePos.x * 10) / 10) % 0.2;
				 bool sliceY = (round(i.worldSpacePos.y * 10) / 10) % 0.2;
				 bool sliceZ = (round(i.worldSpacePos.z * 10) / 10) % 0.2;

				 col = fixed4(col.r, col.g, col.b, col.a * ((sliceX && sliceY && sliceZ) ? 1 : 0));

                 return col;
             }
         ENDCG
     }
 }
 
 }
