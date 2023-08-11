Shader "Sprites/Censored"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
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
			Blend Zero SrcColor

		Pass
		{
			Stencil
			{
				Ref 101
				Comp NotEqual
				Pass Keep
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
				fixed4 color : COLOR;
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
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
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
				IN.texcoord /= IN.zstore;
				fixed4 c1 = SampleSpriteTexture(IN.texcoord + frac(0.2*_Time.zz)) * IN.color;
				fixed4 c2 = SampleSpriteTexture(IN.texcoord - frac(0.15*_Time.zz)) * IN.color;
				fixed4 c = fixed4(c1.rg,0,c1.a*0.5) + fixed4(0,0.75*c2.b,1.5*c2.b,c2.a*0.5);
				c.rgb *= c.a;
				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}