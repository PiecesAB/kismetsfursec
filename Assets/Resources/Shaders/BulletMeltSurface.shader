// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/BulletMeltSurface"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_FakeZ ("Fake 3D Perspective", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite On
		

		Pass
		{

			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Blend One OneMinusSrcAlpha

			Stencil
			{
				Ref 101
				Comp NotEqual
				Pass Keep
			}

		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float zstore : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;
			float4 _FakeZ;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				float oldPosWorld = posWorld.z;
				posWorld.z = 0.0;
				OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
				if (any(_FakeZ.xy))
				{
					float mult = ((_FakeZ.z - _FakeZ.w) / (oldPosWorld - _FakeZ.w));
					OUT.vertex.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
					OUT.vertex.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
				}
				
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				//OUT.vertex.z -= 1.0;
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				half2 offset = half2(frac(sin(_Time.z*0.5)), frac(cos(_Time.z*0.7)));
				fixed4 c = SampleSpriteTexture (IN.texcoord + offset) * IN.color;

				c.rgb += (1. + sin(_Time.z*4.0))*0.1;
			    
				c.rgb *= c.a;

				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
