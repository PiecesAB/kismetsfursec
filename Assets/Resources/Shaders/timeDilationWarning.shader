// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/TimeDilationWarning"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_TDR ("TimeDilationRange", Range(0.25,4.0)) = 2.0
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
				fixed4 color    : COLOR;
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
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			half _TDR;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = _Color;
			half2 uv = IN.texcoord;
			half2 haf = { 0.5, 0.5 };
			half2 zro = { 0.0, 0.0 };
			uv -= haf;
			uv *= 2.0;
			uv.x = min(uv.x, 100000);
			clip(-sqrt(uv.x*uv.x+uv.y*uv.y) + 1.0);
			if (_TDR > 1.179)
			{
				clip(uv.y);
				clip(abs(uv.y / uv.x) - tan((log(2.0*_TDR - 2.0) / 1.7917595) + 0.5707963));
			}
			else if (_TDR < 0.93)
			{
				clip(-uv.y);
				clip(-tan((log(-6.0*_TDR + 6.0) / 1.5040774) + 3.7123890) + abs(uv.y / uv.x));
			}
			else
			{
				clip(0.1227846 - abs(uv.y / uv.x));
			}



				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
