Shader "Sprites/E8CheckerBar"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_C1 ("Checker Color 1", Color) = (1,1,1,1)
		_C2 ("Checker Color 2", Color) = (1,1,1,1)
		_C3 ("Checker Color 3", Color) = (1,1,1,1)
		_Speed ("Checker Scroll Speed", Float) = 1.0
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
			fixed4 _C1;
			fixed4 _C2;
			fixed4 _C3;
			float _Speed;
			float _DTScaledTimeSinceLoad;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				return tex2D(_MainTex, uv);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				float t = _DTScaledTimeSinceLoad * _Speed;
				c = lerp(c, _C3, step(0.5, abs(round((IN.texcoord.x * 4.0 + frac(t * 1.5)) % 1.0) - round((IN.texcoord.y * 4.0) % 1.0))));
				c = lerp(c, _C2, step(0.5, abs(round((IN.texcoord.x * 2.0 + frac(t * 0.5)) % 1.0) - round((IN.texcoord.y * 2.0) % 1.0))));
				c = lerp(c, _C1, step(0.5, abs(round((IN.texcoord.x + frac(t)) % 1.0) - round(IN.texcoord.y % 1.0))));
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
