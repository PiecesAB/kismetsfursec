// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/SunRayShineOnMoon"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_ATex("Lighting A Texture", 2D) = "white" {}
		_LAngle("Lighting A Angle", Float) = 0
		_ACol("Lighting A Color", Color) = (1, 1, 1, 1)
		_BTex("Lighting B Texture", 2D) = "white" {}
		_BCol("Lighting B Color", Color) = (0, 0, 0, 1)
		_Color("Tint", Color) = (1,1,1,1)
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
			sampler2D _ATex;
			sampler2D _BTex;
			float _LAngle;
			float _DTScaledTimeSinceLoad;
			fixed4 _ACol;
			fixed4 _BCol;

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
				polar.y -= _LAngle;
				float2 apos = polar.x * float2(cos(polar.y), sin(polar.y));
				fixed4 c2 = tex2D(_ATex, apos + float2(0.5, 0.5)) * IN.color;
				fixed4 c3 = tex2D(_BTex, IN.texcoord) * IN.color;
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb += c2.r * _ACol.rgb;
				c.rgb += c3.r * _BCol.rgb;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
