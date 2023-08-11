// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enemy/MineExplosion"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Res ("Resolution", Int) = 64
		
		_d ("Duration", Float) = 0.6
		_t("Time at which the explosion started", Float) = -1000
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
#pragma target 3.0
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
			half _Res;
			float _t;
			float _d;
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
			fixed4 c = {0.0,0.0,0.0,0.0};

			half2 uv = IN.texcoord;
			half2 zq = { 0.5, 0.5 };
			uv -= zq;
			uv *= 10.0; //centered and max at 5
			uv = (round(uv*_Res/10.0) / _Res)*10.0;

			half h = (_Time.y-_t) / _d;
			if (h < 1.0 && h >= 0.0)
			{
				half z = (1.0 - h)*(1.0-h);  //1 is beginning. 0 is end
				
				
				if (pow(abs(uv.x), z) + pow(abs(uv.y), z) < pow(z, -z))
				{
					fixed4 add1 = { 0.65,1.0,1.0,0.6 };
					c += add1;
				}
				if (abs(uv.x*uv.x + uv.y*uv.y - 2.0 / z) < 1.0)
				{ 
					fixed4 add2 = { 1.0,1.0,0.0,0.5 };
					c += add2;
				}
					
			}
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
