Shader "Sprites/RainbowPower"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Brightness("Brightness", Float) = 0.0
		_DT("Darkness Cutoff", Range(0.0,1.0)) = 0.3
		_Inten("Intensity",Float) = 0.15
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
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				fixed _Brightness;
				fixed _DT;
				half _Inten;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				if ((c.r + c.g + c.b)*0.3333 < _DT)
				{
					half pn = 1.0 - _Inten;
					c.r *= sin(-1.6*_Time.z + cos(IN.texcoord.y*20.0) + sin(IN.texcoord.x*20.0))*_Inten + pn;
					c.g *= cos(2.8*_Time.z - cos(IN.texcoord.y*20.0) - sin(IN.texcoord.x*20.0))*_Inten + pn;
					c.b *= sin(2.0*_Time.z + sin(IN.texcoord.y*20.0) - cos(IN.texcoord.x*20.0))*_Inten + pn;
					c.rgb *= 1.0 + _Brightness;
				}
					c.rgb *= c.a;
					
					return c;
				}
			ENDCG
			}
		}
}

