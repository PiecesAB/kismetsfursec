// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/MeltCutout"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_StencilId ("Stencil Id (101)", Int) = 101
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry-1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}


		//AlphaTest GEqual[_Cutoff]
		Pass
		{
			
			Cull Off
			Lighting Off
			ZWrite On

			Blend One OneMinusSrcAlpha

			ColorMask 0
			Stencil
			{
				Ref [_StencilId]
				Comp Always
				Pass Replace
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
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float2 wpos : TEXCOORD1;
			};

			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.wpos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float _Cutoff;

			static const half4x4 dot = {
					0.0,8.0,2.0,10.0,
					12.0,4.0,14.0,6.0,
					3.0,11.0,1.0,9.0,
					15.0,7.0,13.0,5.0
			};

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;
				int ax = (int)(floor(frac(IN.wpos.x/4.0)*4.0));
				int ay = (int)(floor(frac(IN.wpos.y / 4.0)*4.0));
				if (c.a*16.0 < dot[ay][ax] + 0.5)
				{
					clip(-1);
				}
				clip(c.a - 0.01);
				return c;
			}
		ENDCG
		}
	}
}
