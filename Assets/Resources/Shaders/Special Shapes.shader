// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Special Shapes"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_FakeZ("Fake 3D Perspective", Vector) = (0,0,0,0)
		_CB("Bottom Color", Color) = (0,0,0,1)
		_CL("Left Color", Color) = (0,1,0,1)
		_CR("Right Color", Color) = (1,0,0,1)
		_CT("Top Color", Color) = (1,1,1,1)
		_CM("Middle Color", Color) = (0.5,0.5,0.5,1)
		_CP("Pattern Color", Color) = (0,1,1,1)
		_CST("Top Slant Color", Color) = (1,1,0,1)
		_CSB("Bottom Slant Color", Color) = (0,0,1,1)
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
			ZWrite On
			Blend One OneMinusSrcAlpha

			Pass
			{
				Stencil
				{
					Ref 197
					Comp NotEqual
					Pass Replace
				}

			CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile_instancing
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
					float zstore : TEXCOORD1;
				};

				fixed4 _Color;
				fixed _AlphaCutoff;
				float4 _FakeZ;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
					posWorld.x += 4.0;
					posWorld.y -= 4.0;
					float oldPosWorld = posWorld.z;
					posWorld.z = 16.0;
					OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
					if (any(_FakeZ.xy))
					{
						float mult = ((_FakeZ.z - _FakeZ.w) / (oldPosWorld - _FakeZ.w));
						OUT.vertex.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
						OUT.vertex.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
					}

					OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
					//OUT.vertex.z += 0.1;
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif
					OUT.texcoord *= OUT.vertex.w;
					OUT.zstore = OUT.vertex.w;
					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					IN.texcoord /= IN.zstore;
					fixed4 c = fixed4(0.0,0.0,0.0,SampleSpriteTexture(IN.texcoord).a*0.4)*IN.color;

					c.rgb *= c.a;

					clip(c.a - _AlphaCutoff);

					return c;
				}
			ENDCG
			}

			Pass
			{
				Stencil
				{
					Ref 255
					Comp Always
					Pass Replace
				}

			CGPROGRAM
				#pragma target 3.0
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
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				fixed4 _CB;
				fixed4 _CL;
				fixed4 _CR;
				fixed4 _CT;
				fixed4 _CM;
				fixed4 _CP;
				fixed4 _CST;
				fixed4 _CSB;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);



					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				fixed x = c.a;
				if (c.r == 1.0)
				{
					if (c.g == 1.0)
					{
						if (c.b == 1.0)
						{
							//white
							c = _CT;
						}
						else
						{
							//yellow
							c = _CST;
						}
					}
					else
					{
						//red
						c = _CR;
					}
				}
				else
				{
					if (c.g == 1.0)
					{
						if (c.b == 1.0)
						{
							//cyan
							c = _CP;
						}
						else
						{
							//green
							c = _CL;
						}
					}
					else
					{
						if (c.b == 1.0)
						{
							//blue
							c = _CSB;
						}
						else
						{
							if (c.r > 0.0)
							{
								//midgray
								c = _CM;
							}
							else
							{
								//black
								c = _CB;
							}
						}
					}
				}
				c.a = x;

					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}
