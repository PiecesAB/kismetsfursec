// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/SurfaceAsteriskVariant"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_TOH("Tired or High Value", Range(-1.0,1.0)) = 0.0
		_Stun("Stunned", Float) = 0.0
		_Shadow("Have Shadow? 1 for yes", Range(0.0,1.0)) = 1.0
		_BW("Black and White? 1 for yes", Range(0.0,1.0)) = 0.0
		_BWCutoff("Black and white cutoff", Range(0.0,1.0)) = 0.5
		_BWNoise("Black and white noise", Range(0.0,1.0)) = 0.0
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
		ZWrite On
		Blend One OneMinusSrcAlpha

		/*Pass
		{
			Stencil
			{
				Ref 197
				Comp NotEqual
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
				fixed4 color : COLOR;
				half2 texcoord  : TEXCOORD0;
				float zstore : TEXCOORD1;
			};

			fixed4 _Color;
			fixed _AlphaCutoff;
			half _Shadow;

			v2f vert(appdata_t IN)
			{
				
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				posWorld.x += 4.0;
				posWorld.y -= 4.0;
				float oldPosWorld = posWorld.z;
				posWorld.z = 16.0;
				OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				//OUT.vertex.z += 1.0;
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				clip(_Shadow - 1.0);
				IN.texcoord /= IN.zstore;
				fixed4 c = fixed4(0.0,0.0,0.0,SampleSpriteTexture(IN.texcoord).a*0.4)*IN.color;

				c.rgb *= c.a;

				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}*/

		Pass
		{
			Stencil
			{
				Ref 69
				Comp Always
				Pass Replace
			}

		CGPROGRAM
			#pragma target 3.0
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
				float zstore : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(unity_ObjectToWorld, IN.vertex);
				float a = frac((_Time.z + OUT.vertex.x*0.01 + OUT.vertex.y*0.01)*0.3);
				a = exp(-a * 4.0)*16.0*sin(a*16.0);
				float b = max(frac((_Time.z + OUT.vertex.x*0.01 + OUT.vertex.y*0.01)*0.15)*2.0 - 1.0, 0.0);
				b = exp(-b * 4.0)*16.0*min(1.0,b*9.0)*cos(b*16.0);
				OUT.vertex.xy += a;
				OUT.vertex.y += b;
				OUT.vertex.x -= b;
				OUT.vertex = mul(UNITY_MATRIX_V, OUT.vertex);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			float _TOH;
			fixed _Stun;
			half _BW;
			fixed _BWCutoff;
			fixed _BWNoise;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

				/*if (_BW) {
					fixed newcut = _BWCutoff + _BWNoise * cos(1140.0*_Time.y) * cos(1389.0*_Time.y);
					c.rgb = (1.0-smoothstep(newcut-0.15, newcut+0.15, (0.3*c.r + 0.59*c.g + 0.11*c.b))) * IN.color.rgb;
				}*/

				c.rgb *= c.a;

				/*c = lerp(c, fixed4(0.8, 0.0, 0.8, c.a), max(-(0.7*_TOH), 0.0));
				c = lerp(c, fixed4(0.8+0.2*sin(3643.0*_Time.y), 0.8 + 0.2*sin(1283.0*_Time.y), 0.8 + 0.2*sin(1928.0*_Time.y), c.a), max((0.7*_TOH), 0.0));
				c = lerp(c, fixed4(fixed3(1.0, 1.0, 0.0)*2.0*sin(48.0*(_Time.y+IN.texcoord.x+IN.texcoord.y)),c.a), _Stun);*/
				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
