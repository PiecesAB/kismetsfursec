// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/SunRayEclipse"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Bounds("Bounds", Vector) = (-1,1,0,0)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
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
			float _DTScaledTimeSinceLoad;
			half2 _Bounds;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				float2 centered = IN.texcoord - float2(0.5, 0.5);
				float2 polar = float2(distance(IN.texcoord, float2(0.5, 0.5)), atan2(centered.y, centered.x));
				float block = smoothstep(-0.1, 0.1, -(polar.y - _Bounds.x)) + smoothstep(-0.1, 0.1, -(_Bounds.y - polar.y)) - 0.1 - step(_Bounds.y, _Bounds.x);
				clip(block);
				block = min(block, 1.0);
				float2 polarb = polar;
				float2 dif = (3.1415926 * 2.0) * ((_DTScaledTimeSinceLoad * 0.15) % 1.0);
				polar.y += dif;
				polarb.y -= dif;
				float2 newcoord = polar.x * float2(cos(polar.y), sin(polar.y));
				float2 newcoordb = polarb.x * float2(cos(polarb.y), sin(polarb.y));
				fixed4 c = SampleSpriteTexture(newcoord + float2(0.5, 0.5)) * IN.color;
				c = max(c, SampleSpriteTexture(newcoordb + float2(0.5, 0.5)) * IN.color);
				c.a *= block;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
