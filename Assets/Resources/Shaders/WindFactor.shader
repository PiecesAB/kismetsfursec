// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/WindFactor"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_Shift("Wind Shift Direction", Float) = -1.0
		_ShiftY("Wind Shift Direction Y", Float) = 0.0
		/*_TOH("Tired or High Value", Range(-1.0,1.0)) = 0.0
		_Stun("Stunned", Float) = 0.0
		_Shadow("Have Shadow? 1 for yes", Range(0.0,1.0)) = 1.0
		_BW("Black and White? 1 for yes", Range(0.0,1.0)) = 0.0
		_BWCutoff("Black and white cutoff", Range(0.0,1.0)) = 0.5
		_BWNoise("Black and white noise", Range(0.0,1.0)) = 0.0
		_TexSize("Texture size (XY) and Offset (ZW)", Vector) = (1.0,1.0,0.0,0.0)
		_WaveDistort("Wave Distortion (XY = displacement, ZW = speed)", Vector) = (0.0,0.0,0.0,0.0)
		_StencilId("Stencil ID (default for this shader is 69)", Int) = 69*/
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
		Blend One One

	    GrabPass
		{
			"_Env"
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
				float4 grabpos : TEXCOORD2;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;
			sampler2D _Env;

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
				OUT.grabpos = ComputeScreenPos(OUT.vertex);
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			half _Shift;
			half _ShiftY;

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

			    fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				fixed a = c.a;
			    c.a = 1.0;
				float4 gp = IN.grabpos;
				gp.x += 0.1*_Shift*a;
				gp.y += 0.1*_ShiftY*a;
				a *= 5.0 * min(0.2, min(IN.texcoord.x, 1.0 - IN.texcoord.x)) * 4.0 * min(0.25, min(IN.texcoord.y, 1.0 - IN.texcoord.y));
				fixed4 s = tex2D(_Env, gp);
				c = lerp(s, c, a);
				a *= min(1.0, c.a * 10.0);
				clip(a - 0.08);
				c -= 0.5 * s;
			    c.rgb *= c.a;
			    clip(c.a - _AlphaCutoff);
			
				return c;
			}
		ENDCG
		}
	}
}
