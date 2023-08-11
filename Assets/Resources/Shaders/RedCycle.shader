// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/RedCycle"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_V("Stencil Value", Int) = 42
		_S("Cycle Speed", Float) = 1.0
		_CC("Cycle Color", Color) = (1,1,1,1)
		_WPT("World Backdrop Texture (1:ON)", Float) = 0.0
		_WPS("World Backdrop Tile Size", Float) = 16.0
		_Solid ("Solid Stripe (1:ON)", Float) = 0.0
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

		Stencil
		{
			Ref[_V]
			Comp Always
			Pass Replace
		}

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
				float2 wpos : TEXCOORD1;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 wposTemp = mul(unity_ObjectToWorld, IN.vertex);
				OUT.wpos = float2(wposTemp.x, wposTemp.y);
				//IN.vertex = mul(unity_WorldToObject, round(OUT.wpos));
				OUT.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, round(wposTemp)));
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

				return color;
			}

			float _S;
			fixed4 _CC;
			fixed _WPT;
			float _WPS;
			fixed _Solid;

			fixed4 frag(v2f IN) : SV_Target
			{
			fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
			if (_WPT >= 0.5)
			{
				c = SampleSpriteTexture((IN.wpos%_WPS)/_WPS) * IN.color;
			}
			if (all(c.ra) && !any(c.gb))
			{
				fixed cycle = ((_S*_Time.z + c.r) % 1.0);
				cycle = lerp(cycle, 0.8*round(cycle), round(_Solid));
				c = cycle*_CC;
			}
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
