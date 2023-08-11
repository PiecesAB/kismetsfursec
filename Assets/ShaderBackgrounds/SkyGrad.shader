Shader "Unlit/SkyGradient"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_CD("Color Depth", Range(0.0,0.9961)) = 0.75
		_Color("Tint", Color) = (1,0,1,1)
		_Res("Resolution", Vector) = (160,240,0,0)
		_BC("Bottom Color", Color) = (0,0,0,1)
		_TC("Top Color", Color) = (1,1,1,1)
		_Curv("Curvature", Range(0.0,1.57)) = 0.4
	    _CMul("Center Multiplier", Float) = 1.0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
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

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma shader_feature ETC1_EXTERNAL_ALPHA
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
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				half2 _Res;
				half _CD;
				fixed4 _BC;
				fixed4 _TC;
				half _Curv;
				half _CMul;

				static const half4x4 dot = {
					0.0,8.0,2.0,10.0,
					12.0,4.0,14.0,6.0,
					3.0,11.0,1.0,9.0,
					15.0,7.0,13.0,5.0
				};

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

	#if ETC1_EXTERNAL_ALPHA
					// get the color from an external texture (usecase: Alpha support for ETC1 on android)
					color.a = tex2D(_AlphaTex, uv).r;
	#endif //ETC1_EXTERNAL_ALPHA

					return color;
				}

				half rnd1(half a)
				{
					return a - (a % (1.0 - _CD));
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					half2 uv = IN.texcoord;
					half2 mid = half2(0.5, 0.5);
					uv -= mid;
					uv.xy *= _CMul;
					uv += mid;
					uv.x = floor(IN.texcoord.x*_Res.x) / _Res.x;
					uv.y = floor(IN.texcoord.y*_Res.y) / _Res.y;
					half2 coord = { uv.x*_Res.x,uv.y*_Res.y };
					half tan1 = tan(abs(uv.x - 0.5)*_Curv);
					fixed4 c = lerp(_BC,_TC,uv.y+(tan1*tan1));
					fixed az = 1.0 - _CD;
					fixed4 un = {rnd1(c.r),rnd1(c.g) ,rnd1(c.b) ,1.0};
					fixed4 ov = { un.r + az, un.g + az, un.b + az, 1.0 };

					fixed4 dif = (az - (ov - c)) / az;

					int ax = (int)(floor(coord.x) % 4.0);
					int ay = (int)(floor(coord.y) % 4.0);

					c = un;

					if (dif.r*16.0 > dot[ay][ax] + 0.5)
					{
						c.r += az;
					}
					if (dif.g*16.0 > dot[3 - ay][ax] + 0.5)
					{
						c.g += az;
					}
					if (dif.b*16.0 > dot[ay][3 - ax] + 0.5)
					{
						c.b += az;
					}

				c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}
