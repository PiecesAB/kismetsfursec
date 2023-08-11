// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Background/LiquidA"
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
			sampler2D _AlphaTex;

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
				uv.x *= 1.33333333;
				uv = floor(uv*240.0) / 240.0;
				half2 center = { 0.66666666,0.5 };

				uv.x += (0.2*sin((2.0*(uv.y + uv.x)) + (_Time.y / 2.5))) - 0.1;
				uv.y += (0.2*cos((2.0*uv.x) + (_Time.y / 2.5))) - 0.1;

				half distx = 16.0*distance(uv.x, center.x);
				distx *= cos(_Time.y / 1.5);
				half disty = 16.0*distance(uv.y, center.y);
				disty += sin(_Time.y / 2.0);
				half dist = distx + disty;

				fixed4 col1 = { 0.3,0.3,0.3,1.0 };
				fixed4 col2 = { 0.0,0.0,0.0,1.0 };

				fixed4 c = lerp(col2, col1, abs(sin(3.14159265*abs(dist - _Time.y)))*0.5);
				//c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
