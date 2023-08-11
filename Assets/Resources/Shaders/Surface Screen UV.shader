// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/Surface Screen Mirror"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_AltTex("Alt Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_TOH("Tired or High Value", Range(-1.0,1.0)) = 0.0
		_Stun("Stunned", Float) = 0.0
		_Shadow("Have Shadow? 1 for yes", Range(0.0,1.0)) = 1.0
		_BW("Black and White? 1 for yes", Range(0.0,1.0)) = 0.0
		_BWCutoff("Black and white cutoff", Range(0.0,1.0)) = 0.5
		_BWNoise("Black and white noise", Range(0.0,1.0)) = 0.0
		_MirrorScale("Mirror Scale", Vector) = (1.0,1.0,0.0,0.0)
		_MirrorOffset("Mirror Offset", Vector) = (1.0,1.0,0.0,0.0)
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
			/*Stencil
			{
				Ref 69
				Comp Always
				Pass Replace
			}*/

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
			float2 _MirrorScale;
			float2 _MirrorOffset;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MV, IN.vertex);
				OUT.texcoord = _MirrorOffset + float2(_MirrorScale.x*OUT.vertex.x/320.0 + 0.5, _MirrorScale.y*OUT.vertex.y/216.0 + 0.5);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
				return OUT;
			}

			sampler2D _AltTex;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_AltTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;

				c.rgb *= c.a;
				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
