// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Sprites/Yellow Replacement"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_RepColor("ReplacementYellow",Color) = (0,0,0,0)
			_Speed("Speed (XY only)", Vector) = (2,2,0,0)
			_Intensity("Intensity (XY only)", Vector) = (0,0,0,0)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
			_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_Shadow ("Make Shadow (1 = ON)", Range(0.0,1.0)) = 1.0
		_FakeZ ("Fake 3D Perspective", Vector) = (0,0,0,0)
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
				Ref 197
				Comp NotEqual
				Pass Replace
			}

		CGPROGRAM

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
			float4 _FakeZ;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				posWorld.x += 4.0;
				posWorld.y -= 4.0;
				float oldPosWorld = posWorld.z;
				posWorld.z = 16.0;
				OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
				if (any(_FakeZ.xy))
				{
					float mult = ((_FakeZ.z - _FakeZ.w) / (oldPosWorld - _FakeZ.w));
					OUT.vertex.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
					OUT.vertex.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
				}

				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				//OUT.vertex.z += 0.1;
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
			float _AlphaSplitEnabled;
			half _Shadow;

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				clip(_Shadow - 0.5);
				IN.texcoord /= IN.zstore;
				fixed4 c = fixed4(0.0,0.0,0.0,SampleSpriteTexture(IN.texcoord).a*0.4)*IN.color;

				c.rgb *= c.a;

				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			//#pragma shader_feature ETC1_EXTERNAL_ALPHA
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
				float zstore : TEXCOORD1;
			};
			
			fixed4 _Color;
			half2 _Speed;
			half2 _Intensity;
			float4 _FakeZ;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				float oldPosWorld = posWorld.z;
				posWorld.z = 0.0;

				float4 n = mul(unity_ObjectToWorld, IN.vertex);
				float4 whatever = { 120.0*_Intensity.x*sin(_Speed.x*_Time.z + n.y*0.6),120.0*_Intensity.y*cos(_Speed.y*_Time.z + n.x*0.6),0.0,0.0 };
				n += whatever;
				OUT.vertex = mul(UNITY_MATRIX_V, n);
				if (any(_FakeZ.xy))
				{
					float mult = ((_FakeZ.z - _FakeZ.w) / (oldPosWorld - _FakeZ.w));
					OUT.vertex.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
					OUT.vertex.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
				}

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
			sampler2D _AlphaTex;
			fixed4 _RepColor;
			fixed _AlphaCutoff;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

//#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				//color.a = tex2D (_AlphaTex, uv).r;
//#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{ 
				IN.texcoord /= IN.zstore;
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			if (abs(c.r - c.g) < 0.01 && c.b < c.g && c.a != 0.0)
			{
				fixed z = c.r;
				fixed n = c.b;
				fixed4 c2 = _RepColor*(z+2.0*c.b);
				c2.a = c.a;
				c = c2;
			}
				c.rgb *= c.a;
				clip(c.a - _AlphaCutoff);
				return c;
			}
		ENDCG
		}
	}
}
