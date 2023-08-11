// Simplified Alpha Blended Particle shader. Differences from regular Alpha Blended Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Mobile/Particles/Alpha Blended Intvert" {
Properties {
	_Color("Tint", Color) = (1,1,1,1)
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		/*Pass {
			SetTexture [_MainTex] {
				combine texture * primary
			}
		}*/

		Pass {
			CGPROGRAM
			 #include "UnityCG.cginc"
			 #pragma vertex vert addshadow
			 #pragma fragment frag
			 #pragma multi_compile_fog
			 #pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF

			 sampler2D _MainTex;
			 float4 _MainTex_ST;

			 struct appdata_t {
				 float4 vertex : POSITION;
				 float2 texcoord : TEXCOORD0;
				 float4 color    : COLOR;
				 float2 texcoord1 : TEXCOORD1;
			 };

			 struct v2f {
				 float4 vertex : SV_POSITION;
				 float2 uv : TEXCOORD0;
				 half2 uv2 : TEXCOORD1;
				 fixed4 color : COLOR;
				 UNITY_FOG_COORDS(2)
			 };

			 fixed4 _Color;

			 v2f vert(appdata_t v)
			 {
				 v2f o;
				 float4 vPos = mul(UNITY_MATRIX_MV, v.vertex);
				 o.vertex = mul(UNITY_MATRIX_P, float4(round(vPos.xyz * 4.0) * 0.25, vPos.w));
				 o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				 o.uv2 = v.texcoord;
				 o.color = v.color * _Color;
				 UNITY_TRANSFER_FOG(o,o.vertex);

				 return o;
			 }

			 half4 frag(v2f i) : COLOR
			 {
				 half4 col = tex2D(_MainTex, i.uv.xy) * i.color;

				 UNITY_APPLY_FOG(i.fogCoord, col);

				 return col;
			 }
			 ENDCG
		}
	}
}
}
