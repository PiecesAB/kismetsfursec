// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Important/WholeScreenTex"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_DI1 ("Perlin Distortion Intensity 1", Float) = 0.1
		_BlackBorder ("Black Border Size", Float) = 0.0
		_SolidColor ("Solid Color (Special effect, use black BG, enable using alpha > 0)", Color) = (0,0,0,0)
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
			
			// Inigo Quilez - iq/2013
			// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

			float hash(float n)
			{
				return frac(sin(sin(n)));
			}

			float noise(float3 x)
			{
				// The noise function returns a value in the range -1.0f -> 1.0f

				float3 p = floor(x);
				float3 f = frac(x);

				f = f * f*(3.0 - 2.0*f);
				float n = p.x + p.y*57.0 + 113.0*p.z;

				return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
					lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
					lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
						lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
			}

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
			half _DI1;
			half _BlackBorder;
			fixed4 _SolidColor;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;
				fixed finalMul = 1.0;
				if (_DI1 > 0.0)
				{
					uv.x = round(uv.x*320.0) / 320.0;
					uv.y = round(uv.y*216.0) / 216.0;
				}
				if (_BlackBorder > 0.0)
				{
					uv.x = round(uv.x*320.0) / 320.0 + 0.00001;
					uv.y = round(uv.y*216.0) / 216.0 + 0.00001;
					float ty = _Time.y*0.2;
					float2 uvbb = float2(uv.x + 0.01*sin((uv.y + ty)*12.0) +0.01*cos((uv.y + 0.5 - ty)*7.0), 
										 uv.y + 0.01*sin((uv.x - ty)*12.0) - 0.01*cos((uv.x + 5.8 - ty)*7.0)
									);
					float d1 = min(min((1.0 - uvbb.x), uvbb.x), min((1.0 - uvbb.y), uvbb.y));
					float d2 = d1 * (1.0 / _BlackBorder);
					finalMul = min(1.0, d2);
					half ang = atan2(uv.y - 0.5, uv.x - 0.5);
					uv += 0.2*(3.0- min(3.0, d2))*d1*float2(cos(ang), sin(ang));
				}
				float2 coord = (uv + _DI1 * noise(uv.xyx*9.0 + sin(_Time.zyz))) - (_DI1*0.5);
				fixed4 c = SampleSpriteTexture(coord) * IN.color;
				//c += 50.0*ddx(uv.x) + 50.0*ddy(uv.y);

				c.rgb *= c.a;
				c.rgb *= finalMul;

				if (_SolidColor.a > 0.01 && any(c.rgb)) { c.rgb = _SolidColor.rgb*(0.5 + 0.15*c.r + 0.295*c.g + 0.065*c.b); }

				return c;
			}
		ENDCG
		}
	}
}
