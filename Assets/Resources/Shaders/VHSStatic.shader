// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/VHSStatic"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_R ("Resolution", Vector) = (1.0,1.0,0.0,0.0)
		_T ("Transparency", Float) = 0.0
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
			float2 _R;
			fixed _T;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord;
				uv.x = floor(uv.x*_R.x)/_R.x + floor(_Time.y*60.0)/60.0;
				uv.y = floor(uv.y*_R.y)/_R.y + floor(_Time.x*60.0) / 60.0;
				half c5 = frac(sin(dot(uv, float2(12.17634, 79.40319))) * 1268.23);
				fixed4 c = lerp(fixed4(c5, c5, c5, 1.0), fixed4(1.0,1.0,1.0,c5),_T)*IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
