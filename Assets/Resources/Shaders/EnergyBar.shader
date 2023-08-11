// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/EnergyBar"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Val("Actual Value", Range(0.0,1.0)) = 1.0
		_LerpVal("Lerped Value", Range(0.0,1.0)) = 1.0
		_BarColor("Bar Color", Color) = (0,1,0,1)

			_ColorMask("Color Mask", Float) = 15
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
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

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
			fixed4 _BarColor;
			float _Val;
			float _LerpVal;

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
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				float2 lol = IN.texcoord;
				lol.y = floor(lol.y*10.0) / 10.0;
				lol.x = floor(lol.x*54.0) / 54.0;

				if (c.b < 0.6 && c.a > 0.6)
				{
					if (lol.x <= _LerpVal)
					{
						c = _BarColor;

						float v = (0.7 + (0.3*(step(1.0, ((lol.y*4.0+sin(lol.x*21.6+_Time.z*1.2)*cos(_Time.z*1.2)) % 2.0 )))))*(1.0 - abs(lol.y - 0.5));
						c *= v;
					}
					if (_LerpVal < _Val && lol.x < _Val && _LerpVal < lol.x)
					{
						fixed4 green = { 0.0,1.0,0.0,1.0 };
						c = green;
					}
					if (_LerpVal > _Val && lol.x > _Val && _LerpVal > lol.x)
					{
						fixed4 red = { 1.0,0.0,0.0,1.0 };
						c = red;
					}
				}

				return c;
			}
		ENDCG
		}
	}
}
