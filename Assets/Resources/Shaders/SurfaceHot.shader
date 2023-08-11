// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Splatter/SurfaceHot"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Heat("Blackbody Heat", Float) = 0.5
		_PulseSpeed("Pulse Speed", Float) = 0.5
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_AlphaCutoff("Alpha Cutoff", Range(0.01, 1.0)) = 0.01
		_FakeZ ("Fake 3D Perspective", Vector) = (0,0,0,0)
		_Shadow("Has Shadow (1 = on, 0 = off)", Float) = 1.0
		_WorldZ("Z plane", Float) = 0.0
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
			#pragma multi_compile_instancing
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
			float _WorldZ;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				posWorld.x += 4.0;
				posWorld.y -= 4.0;
				float oldPosWorld = posWorld.z;
				posWorld.z = _WorldZ + 16.0;
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
			fixed _Shadow;

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
			Stencil
			{
				Ref 255
				Comp Always
				Pass Replace
			}

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
				half2 texcoord  : TEXCOORD0;
				float zstore : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed _AlphaCutoff;
			float4 _FakeZ;
			float _WorldZ;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				float oldPosWorld = posWorld.z;
				posWorld.z = _WorldZ;
				OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
				if (any(_FakeZ.xy))
				{
					float mult = ((_FakeZ.z - _FakeZ.w) / (oldPosWorld - _FakeZ.w));
					OUT.vertex.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
					OUT.vertex.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
				}
				
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				//OUT.vertex.z -= 1.0;
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
			float _AlphaSplitEnabled;
			half _Heat;
			half _PulseSpeed;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
			    
				c.rgb *= c.a;

				half hf = 0.5 * _Heat * (cos(_Time.z * _PulseSpeed) + 1.0);
				c.r += hf;
				c.g += hf * hf;

				clip(c.a - _AlphaCutoff);

				return c;
			}
		ENDCG
		}
	}
}
