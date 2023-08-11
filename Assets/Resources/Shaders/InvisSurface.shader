// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/InvisSurface"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_Dist("From what radius am I visible?",Float) = 64.0
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

		GrabPass{ "_GrabTexture" }

		Pass
		{
			Stencil
			{
				Ref 5
				Comp Always
				Pass Replace
			}

		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
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
				half2 texcoord  : TEXCOORD0;
				float4 grabpos  : TEXCOORD1;
				float4 wspace   : TEXCOORD2;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;
			half2 _PlayerPositions[4];

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.wspace = mul(unity_ObjectToWorld, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				OUT.grabpos = ComputeGrabScreenPos(OUT.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			sampler2D _GrabTexture;
			half _Dist;
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2Dproj(_GrabTexture, IN.grabpos);
				fixed4 nc = SampleSpriteTexture (IN.texcoord) * IN.color;
				c = lerp(c, nc, step(0.0, nc.a));
				
				for (int i = 0; i < 1; i++)
				{
					if (any(_PlayerPositions[i])) //nonzero
					{
						half di = distance(_PlayerPositions[i],  IN.wspace);
						c.a *= (sin((di*0.2)-16.0*_Time.z)*0.2)+(0.8*(1.0-(di/_Dist)));
						c.a = max(0.0, c.a);
					}
				}
				c.rgb *= c.a;
				//c.rgb = min(c.a*50.0, c.rgb);
				clip(c.a /*- _AlphaCutoff*/);
				clip(nc.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
