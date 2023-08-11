// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/ThetaPlatform"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_BGColor ("Background Color", Color) = (1,1,1,1)
		_Radius ("Radius", Float) = 4.0
	    _Size ("Normal Size", Float) = 8.0
		_Div ("Divisions", Int) = 16
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
			#pragma target 3.0
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
				//float size : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _BGColor;
			half _Radius;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				//OUT.size = abs(IN.vertex.x);
				OUT.texcoord = IN.vertex.xy;
				IN.vertex.xy *= _Radius;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;

			/*fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				return color;
			}*/

			float _Size;
			int _Div;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord.xy / _Size;
				half d = distance(uv, float2(0.0, 0.0));
				clip(1.0 - d); //circle
				half angle = atan2(uv.y, uv.x);
				if (angle < 0.0)
				{
					angle += 6.2831853;
				}
				half s = 6.2831853 / _Div;
				half s2 = s * 2.0;
				fixed4 c = _BGColor;
				if (angle%s2 >= s)
				{
					c.rgb *= 0.4;
				}
					half light = cos(angle + 0.78539816);
					half d2 = d * d;
					half d4 = d2 * d2;
					c.rg += d4*0.12*light;
					c.b += d4*0.2*light;
				c.a *= 1.0;
				c.rgb *= c.a;
				c.rgb = round(c.rgb*64.0) / 64.0;
				return c;
			}
		ENDCG
		}

		/*Pass
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
				fixed4 color : COLOR;
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
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}*/
	}
}
