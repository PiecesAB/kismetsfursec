// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Danmaku/SimpleCircle"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "black" {}
	    _BTex("Bullet Texture", 2D) = "white" {}
		_BNum("Bullet Amount (don't go over 64)", Int) = 15
		_BSize("Bullet Size", Float) = 8
		_RAV("Turning Velocity", Float) = -23
		_RAA("Turning Latency", Float) = 0.01
		_Speed("Bullet Speed", Float) = 40
		_Start("Initialization Time", Float) = 0
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
			static const float _tau = 6.2831853;
			sampler2D _BTex;
			int _BNum;
			half _BSize;
			half _RAV;
			half _RAA;
			half _Speed;
			float _Start;

			half2 _positions[64];

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D(_BTex, uv);
				color.rgb *= color.a;
#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				//color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = {0.0,0.0,0.0,0.0};

				for (int i = 0; i < _BNum; i++)
				{
					float t1 = _Time.y - _Start;
					half z = ((float(i) / float(_BNum)) + (_RAV*pow(t1, _RAA)))*_tau;
					half2 ok = { cos(z)*_Speed*t1,sin(z)*_Speed*t1 };
					_positions[i] = ok;
				}

				half2 halfandhalf = { 0.5, 0.5 };
				half2 uv = (IN.texcoord- halfandhalf)*640.0;

				for (int i = 0; i < _BNum; i++)
				{
					half2 pos = _positions[i];
					half x = uv.x - pos.x;
					half y = uv.y - pos.y;
					if (max(abs(x), abs(y)) <= _BSize)
					{
						half2 lol = { x, y };

						c += SampleSpriteTexture((lol / (2.0*_BSize)) + halfandhalf);

					}

				}





				return c;
			}
		ENDCG
		}
	}
}
