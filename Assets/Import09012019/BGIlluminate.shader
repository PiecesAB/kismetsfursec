Shader "Sprites/Effects/BGIlluminate"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Brightness("Brightness", Float) = 0.0
		_XRes("X resolution", Float) = 48.0
		_Inten("Intensity",Float) = 0.15
		_Speed("Scroll Speed", Float) = 1.0
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
				float _XRes;
				half _Inten;
				half _Speed;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

					return color;
				}

				float JunkRand(float x)
				{
					x += 7.0;
					float4 fun = float4(cos(sin(x))-0.1263561, sin((x+0.3475643)*11.519138), cos(cos(x))*frac(x), -cos(x*x));
					return frac(dot(fun, fun-1.0));
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					half2 uv = IN.texcoord;
					uv.xy = floor(uv.xy*_XRes) / _XRes;
					half pn = 1.0 - _Inten;
					c.r *= sin(-1.6*_Time.z + cos(IN.texcoord.y*20.0) + sin(IN.texcoord.x*20.0))*_Inten + pn;
					c.g *= cos(2.8*_Time.z - cos(IN.texcoord.y*20.0) - sin(IN.texcoord.x*20.0))*_Inten + pn;
					c.b *= sin(2.0*_Time.z + sin(IN.texcoord.y*20.0) - cos(IN.texcoord.x*20.0))*_Inten + pn;
					c.a *= JunkRand(20.0*(uv.x-0.0004*uv.y+(_Time.x*0.04*_Speed)))*(1.0-uv.y);
					c.rgb *= c.a;
					c.rgb *= 1.0+_Brightness;
					return c;
				}
			ENDCG
			}
		}
}

