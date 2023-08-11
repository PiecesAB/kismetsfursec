// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/GateOffset"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Open ("Openness", Float) = 0
			_HC("hclip", Float) = 1
		_Rev (">1 for right <1 for left", Float) = 0
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
		ZWrite On
		Blend One OneMinusSrcAlpha

		Pass
		{
			// dont use this! the real stencil is in the real pass, not the shadow.
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

			v2f vert(appdata_t IN)
			{

				v2f OUT;
				float4 posWorld = mul(unity_ObjectToWorld, IN.vertex);
				posWorld.x += 4.0;
				posWorld.y -= 4.0;
				float oldPosWorld = posWorld.z;
				posWorld.z = 16.0;
				OUT.vertex = mul(UNITY_MATRIX_V,posWorld);
				OUT.vertex = mul(UNITY_MATRIX_P, float4(round(OUT.vertex.xyz), OUT.vertex.w));
				//OUT.vertex.z += 1.0;
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

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;

				return color;
			}

			half _Open;
			half _Rev;
			half _HC;
			half _Mul;

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;

				half2 newUv = { 0.0,IN.texcoord.y };
				half z = _Open;
				if (_Rev >= 1.0)
				{
					newUv.x = IN.texcoord.x + z;
					clip(_HC - IN.texcoord.x - z);
				}
				else
				{
					newUv.x = IN.texcoord.x - z;
					clip(IN.texcoord.x - z - _HC);
				}


				fixed4 c = fixed4(0.0,0.0,0.0,SampleSpriteTexture(newUv).a*0.4)*IN.color;

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
			half _Open;
			half _Rev;
			half _HC;
			half _Mul;

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
				half2 newUv = {0.0,IN.texcoord.y };
				half z = _Open;
				if (_Rev >= 1.0)
				{
					newUv.x = IN.texcoord.x + z;
					clip(_HC - IN.texcoord.x - z);
				}
				else
				{
					newUv.x = IN.texcoord.x - z;
					clip(IN.texcoord.x - z - _HC);
				}
				
				fixed4 c = SampleSpriteTexture (newUv) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
