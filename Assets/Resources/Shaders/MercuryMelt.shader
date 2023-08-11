// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/MercuryMelt"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_PI ("Puddle Effect Intensity", Float) = 0.5
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
			#pragma target 3.0
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
			float _PI;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				IN.vertex.xy = mul(unity_ObjectToWorld,mul(unity_WorldToObject, IN.vertex.xy) *4.0* (1.0+_PI));
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = (IN.texcoord-0.5) *4.0 + 0.5;
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

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float p2 = -_PI*2.0;
				float2 tcn = IN.texcoord - 0.5;
				float2 lol = 0.3*float2(sin(4.0*(_Time.z + 1.19373890*tcn.x + 1.0326346*tcn.y)), cos(4.0*(_Time.z - 1.3754323*tcn.x - 0.83746249*tcn.y)));
				float angle = atan2(tcn.y, tcn.x);
				float bend = cos(2.0*angle)*0.29289322 + 0.70710678;
				tcn /= bend; //now it's circle
				tcn = lerp(IN.texcoord, tcn + 0.5, p2);
				clip((1.0 - tcn.x)*tcn.x);
				clip((1.0 - tcn.y)*tcn.y);
				fixed4 c = SampleSpriteTexture (tcn +1.5*_PI*lol) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
