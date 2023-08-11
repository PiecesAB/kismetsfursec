// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Backgrounds/Tunnel1"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
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
			static const float ft = 1.3333333;
			static const float tau = 6.2831853;
			static const half2 center = {0.5,0.5};
			static const half2 zero2 = { 0.0,0.0 };

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
				half2 uv = IN.texcoord;

				uv -= center;
				uv.y /= ft;
				uv = round(uv*160.0) / 160.0;
				fixed4 c = {0.0,0.0,0.0,1.0};

				fixed4 col1 = { 0.03 + 0.06*uv.y,0.03,0.03,1.0 };
				fixed4 col2 = { 0.01 + 0.03*uv.y,0.0,0.01,1.0 };

				if (/*(log2(pow(abs(uv.x), 0.8) + pow(abs(uv.y), 0.8)) - (_Time.y / 4.0) + 64.0)%0.5 > 0.35*/0==1)
				{
					c = col1;
				}
				else
				{
					half angle = atan2(uv.y, uv.x);
					angle = lerp(angle, angle + tau, step(0.0, angle));
					half dist = distance(uv, zero2)*30.0;
					half res = (dist + angle + _Time.y*3.0)%tau;

					c = lerp(c, col2, step(res,tau / 4.0));
				}
				
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
