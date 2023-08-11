Shader "Sprites/Effects/SuperShine"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_ICS("Inner Center Size", Range(0.0,0.5)) = 0.2
		_OCS("Outer Center Size", Range(0.0,0.5)) = 0.3
		_SSZ("Spoke Angular Size", Range(0.05,3.14159265)) = 0.62831853
		_SSP("Spoke Angular Speed", Float) = 1.0
		_Brightness("Brightness", Float) = 0.0
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
				#pragma target 3.0
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
				fixed _Brightness;

				half _ICS;
				half _OCS;
				float _SSZ;
				float _SSP;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					half2 uv = IN.texcoord;
					uv += half2(-0.5, -0.5);
					half distFromCenter = distance(uv, half2(0.0, 0.0));
					clip(0.5 - distFromCenter);
					clip(distFromCenter - _ICS);

					fixed4 c = IN.color;

					if (distFromCenter > _OCS)
					{
						c = fixed4(0.0, 0.0, 0.0, 0.0);
						//code for shine
						half ang1 = atan2(uv.y, uv.x)+3.14159265;
						if (frac((ang1/_SSZ)+(_SSP*_Time.z)) >= asin(sin(-1.6*_Time.z*_SSP - 0.4367))*0.1591549 + 0.5)
						{
							c = IN.color;
						}
					}
					
					c.rgb *= c.a;
					c.rgb *= 1.0+_Brightness;
					return c;
				}
			ENDCG
			}
		}
}

