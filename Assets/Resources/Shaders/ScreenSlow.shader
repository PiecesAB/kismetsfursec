// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ScreenEffect/SlowTime"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_TS ("Timescale", Float) = 1.0
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
			float4 _MainTex_TexelSize;
			sampler2D _AlphaTex;
			half _TS;

			float2 RoundCoords(float2 uv)
			{
				float xs = _MainTex_TexelSize.z *240.0 / _MainTex_TexelSize.w;
				return float2(round(uv.x*xs) / xs, round(uv.y*240.0) / 240.0);
			}

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
				float2 uv = IN.texcoord;
				uv.xy -= 0.5;
				uv.xy = uv.xy/(1.0 + (0.0125*cos(_Time.z / _TS + 6.327463278*IN.texcoord.y - 7.914373233*IN.texcoord.x) /*+ 0.0125*/)/_TS);
				uv.xy += 0.5;
				fixed4 c = SampleSpriteTexture (uv) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
