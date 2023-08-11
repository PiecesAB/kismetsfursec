// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/Surface 1"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_TOH("Tired or High Value", Range(-1.0,1.0)) = 0.0
		_Stun("Stunned", Float) = 0.0
		_Shadow("Have Shadow? 1 for yes", Range(0.0,1.0)) = 1.0
		_BW("Black and White? 1 for yes", Range(0.0,1.0)) = 0.0
		_Tail("Show tail (magenta)?", Range(0.0,1.0)) = 0.0
		_BWCutoff("Black and white cutoff", Range(0.0,1.0)) = 0.5
		_BWNoise("Black and white noise", Range(0.0,1.0)) = 0.0
		_TexSize("Texture size (XY) and Offset (ZW)", Vector) = (1.0,1.0,0.0,0.0)
		_WaveDistort("Wave Distortion (XY = displacement, ZW = speed)", Vector) = (0.0,0.0,0.0,0.0)
		_StencilId("Stencil ID (default for this shader is 69)", Int) = 69
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

		Pass
		{
			// dont use this! the real stencil is in the real pass, not the shadow.
			Stencil
			{
				Ref 62
				Comp Always
				Pass Replace
			}

		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile_instancing
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
		}

		Pass
		{
			Stencil
			{
				Ref [_StencilId]
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
				OUT.vertex = mul(UNITY_MATRIX_MV, IN.vertex);
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
			half4 _TexSize;
			half4 _WaveDistort;
			half _Tail;

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
			    IN.texcoord.xy *= _TexSize.xy;
				IN.texcoord.x += _WaveDistort.x * sin(48.0*IN.texcoord.y + 22612.0 - 2.0*cos(_WaveDistort.z*_Time.y + IN.texcoord.y*120.0));
				IN.texcoord.y += _WaveDistort.y * cos(22.0*IN.texcoord.x + 22442.0 - 2.0*sin(_WaveDistort.w*_Time.y + IN.texcoord.x*600.0));

				fixed4 c = SampleSpriteTexture (IN.texcoord + _TexSize.zw) * IN.color;

				if (_BW) {
					fixed newcut = _BWCutoff + _BWNoise * cos(1140.0*_Time.y) * cos(1389.0*_Time.y);
					c.rgb = (1.0-smoothstep(newcut-0.15, newcut+0.15, (0.3*c.r + 0.59*c.g + 0.11*c.b))) * IN.color.rgb;
				}

				half notmagenta = c.g + abs(1.0 - c.r) + abs(1.0 - c.b);
				clip(-0.01 + notmagenta + _Tail + _BW);
				c = lerp(c, fixed4(0.0, 0.8 + cos(_Time.z * 2.8) * 0.2, 0.0, c.a), clamp(100.0 * (0.01 - notmagenta), 0.0, 1.0));

				c.rgb *= c.a;

				c = lerp(c, fixed4(0.8, 0.0, 0.8, c.a), max(-(0.7*_TOH), 0.0));
				c = lerp(c, fixed4(0.8+0.2*sin(3643.0*_Time.y), 0.8 + 0.2*sin(1283.0*_Time.y), 0.8 + 0.2*sin(1928.0*_Time.y), c.a), max((0.7*_TOH), 0.0));
				c = lerp(c, fixed4(fixed3(1.0, 1.0, 0.0)*2.0*sin(48.0*(_Time.y+IN.texcoord.x+IN.texcoord.y)),c.a), _Stun);
				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
