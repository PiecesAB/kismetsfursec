// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/NebulaShine"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		LOD 200
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			// make fog work
			#pragma multi_compile_fog
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float r : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float zstore : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				float4 fd = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(UNITY_MATRIX_MV, v.vertex);
				o.vertex = mul(UNITY_MATRIX_P, float4(round(o.vertex.xyz), o.vertex.w));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.r = length(float3(v.vertex.x, v.vertex.y, v.vertex.z));
				o.uv *= o.vertex.w;
				o.zstore = o.vertex.w;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 _Tint;
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				i.uv /= i.zstore;
				float2 centered = float2(2.0*i.uv.x - 1.0, 2.0*i.uv.y - 1.0);
				float r = sqrt(centered.x*centered.x + centered.y*centered.y);
				float t = atan2(centered.y, centered.x);
				float v = 0.7 - r - 0.15*sin(_Time.z)*sin(5.0*t + _Time.z);
				clip(v);
				fixed4 col = tex2D(_MainTex, float2(0.5 + 0.5*r*cos(t - _Time.z), 0.5 + 0.5*r*sin(t - _Time.z)));
				col.rgb *= col.a;
				col.rgb *= _Tint.rgb;
				col.rgb *= min(max(v*8.0, 0.0), 1.0);
				col.rgb *= _Tint.a;
				return col;
			}
			ENDCG
		}
	}
}
