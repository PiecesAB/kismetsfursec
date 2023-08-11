// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Sprites...?/WithIntVert"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Affine ("Affine Texture Coords", Float) = 1
		_UIRound ("Round in world space", Float) = 0
		_TeSi ("Texture Size", Vector) = (48,48,0,0)
		_Transition ("Transition", Float) = 1.0
			_ColorMask("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"DisableBatching" = "True"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass //thanks to antonkudin from github
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
				float4 vertex   : POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float zd		: TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float4 origVert : TEXCOORD3;
			};
			
			float2 quant(float2 q, float2 v) {
				return round(q / v)*v;
			}

			float2 quantToWorld(float2 value, float q) {
				float2 wp = mul(unity_ObjectToWorld, float4(value, 0, 0));
				wp = quant(wp, q);
				return mul(unity_WorldToObject, float4(wp, 0, 0));
			}

			fixed4 _Color;
			fixed _Affine;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			fixed _UIRound;
			float2 _TeSi;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.origVert = round(IN.vertex);
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.worldPos = lerp(OUT.worldPos, round(OUT.worldPos), _UIRound);
				OUT.vertex = mul(UNITY_MATRIX_VP, OUT.worldPos);
				OUT.vertex.xy = lerp(round(OUT.vertex.xy), OUT.vertex.xy,_UIRound);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord,_MainTex)*(_Affine*OUT.vertex.z + 1.0-_Affine);
				OUT.zd = OUT.vertex.z;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			
			sampler2D _AlphaTex;
			half _Transition;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{

				IN.texcoord /= ( _Affine*IN.zd + 1.0-_Affine);
				//float2 center = float2(0.5, 0.5);
				//IN.texcoord = quantToWorld(IN.texcoord - center, _MainTex_TexelSize.x*0.5) + center;
				float2 coordsnew = round(IN.texcoord*_TeSi) / _TeSi;
				clip((_Transition*2.0) - (coordsnew.x + coordsnew.y));
				fixed4 c = SampleSpriteTexture (coordsnew) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
