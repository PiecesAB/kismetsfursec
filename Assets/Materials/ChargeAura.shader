// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ChargeAura"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		_Attract("Attraction Speed", Float) = 1.0
		_Spin("Spin Speed", Float) = 0.0
		_FRStart("Fade Radius Start", Float) = 0.5
		_FREnd("Fade Radius End", Float) = 1.0
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
		Blend One OneMinusSrcAlpha

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

			float _Attract;
			float _Spin;
			float _FRStart;
			float _FREnd;
			fixed4 _Tint;
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				i.uv /= i.zstore;
				fixed4 col = tex2D(_MainTex, i.uv + float2(frac(_Spin*_Time.z), frac(_Attract*_Time.z)));
				float rat = (i.r*0.67 - _FRStart)/(_FREnd - _FRStart);
				col.a *= clamp(1.0 - rat, 0, 1);
				col.rgb *= col.a;
				col.rgb *= _Tint;
				col *= _Tint.a;
				return col;
			}
			ENDCG
		}
	}
}
