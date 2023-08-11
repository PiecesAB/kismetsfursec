// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/TransitionA"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_T ("Progress", Range(0.0,1.0)) = 0.0
		_R ("Direction 1 for Out -1 for In", Float) = 1.0
		_V ("Stencil ID", Int) = 43
		_Grayscale ("Grayscale", Float) = 0.0
		_M ("UV Repetition", Float) = 4.0
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

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			float _T;
			fixed _R;
			half _M;
			fixed _Grayscale;

			fixed4 frag(v2f IN) : SV_Target
			{
			float2 uv = IN.texcoord;
			uv = (uv*_M) % 1.0;
			uv.x = ((floor(uv.x*320.0)/320.0)-0.5)*-8.0;
			uv.y = ((floor(uv.y*240.0) / 240.0) - 0.5)*6.0*_R;
			half t = (_T*16.0) - 5.0;
			half ab = _R*(abs((uv.y - floor(uv.y)) - (uv.x - floor(uv.x))) - 0.1 - ((uv.x + t) / 16.0) - ((uv.y + (0.75*t)) / 12.0));
			clip(ab);
			fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
			c.rgb *= c.a;
			fixed gray = 0.3 * c.r + 0.59 * c.g + 0.11 * c.b;
			fixed4 cg = fixed4(gray, gray, gray, c.a);
			c = lerp(c, cg, _Grayscale);
			return c;
			}
		ENDCG
		}
	}
}
