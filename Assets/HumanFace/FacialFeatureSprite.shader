// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/FacialFeatureSprite"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_RedRep("Red Replacement", Color) = (1,0,0,1)
		_BlueRep("Blue Replacement", Color) = (0,0,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite On
		Blend One OneMinusSrcAlpha

		Pass
		{

		CGPROGRAM
			#pragma target 3.0
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

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MV, IN.vertex);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz * 2.0) * 0.5, OUT.vertex.w));
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
			fixed4 _RedRep;
			fixed4 _BlueRep;

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
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				half pureRed = c.r + c.a - c.g - c.b;
				half pureBlue = c.b + c.a - c.r - c.g;
				c = lerp(c, _RedRep, step(1.99, pureRed));
				c = lerp(c, _BlueRep, step(1.99, pureBlue));
				c.rgb *= c.a;
				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
