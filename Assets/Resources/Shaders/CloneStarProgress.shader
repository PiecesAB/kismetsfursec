// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/CloneStarProgress"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_Shadow("Have Shadow? 1 for yes", Range(0.0,1.0)) = 1.0
		_TexSize("Texture size (XY) and Offset (ZW)", Vector) = (1.0,1.0,0.0,0.0)
		_Prog("Progress World Y Position", Float) = -700.0
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
				float3 posStore : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float2 tempWPos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.vertex = mul(UNITY_MATRIX_MV, IN.vertex);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.posStore = float3(tempWPos.x, tempWPos.y, OUT.vertex.w);
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			half4 _TexSize;
			float _Prog;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.posStore.z;
			    IN.texcoord.xy *= _TexSize.xy;

				fixed4 c = SampleSpriteTexture (IN.texcoord + _TexSize.zw) * IN.color;

				if (IN.posStore.y <= _Prog) { c.rgb += 0.3; c.a *= 1.0 - 0.5*frac(_Time.z); }
				else { clip(-1); }
				c.rgb *= c.a;

				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
