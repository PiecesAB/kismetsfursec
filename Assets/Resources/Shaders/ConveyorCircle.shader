// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/ConveyorCircle"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_ConvCol("Conveyor Color", Color) = (0.3,0.3,0.3,1.0)
		_SizeMult("Size Multiplier", Float) = 8.0
		_Speed("Conveyor Speed", Float) = 1.0
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
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				OUT.texcoord *= OUT.vertex.w;
				OUT.zstore = OUT.vertex.w;
				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			fixed4 _ConvCol;
			half _SizeMult;
			half _Speed;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				IN.texcoord /= IN.zstore;
				half2 newCoord = (IN.texcoord*_SizeMult) % 1.0;
				newCoord.xy -= 0.5;
				half ang = atan2(newCoord.y, newCoord.x);
				

				fixed4 c = tex2D(_MainTex, IN.texcoord) * _ConvCol;
				c.rgb += 0.2*floor(0.999*sin(3.0*ang + 19.0*_Speed*_Time.z));
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
