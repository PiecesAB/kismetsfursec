// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Enemy/CircleSweep"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
			_C1("Circle Color TopRight", Color) = (1,1,1,1)
			_C2("Circle Color BottomLeft", Color) = (1,1,1,1)
			_Thicc("Thicc", Float) = 0.1
			_TS("Thiccness Alteration Speed", Float) = 0.5
			_TA("Thiccness Alteration Amount", Float) = 0
			_Speed("Speed", Float) = 1.0
			_Repeatedness("Repeatedness", Int) = 1
			_Res ("Resolution", Int) = 64
		_HoleO ("HoleOffset", Float) = 0.0
		_HoleS ("HoleSize", Float) = 0.5
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
			#pragma target 3.0
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
			fixed4 _C1;
			fixed4 _C2;
			half _Thicc;
			half _HoleS;
			half _HoleO;
			half _Speed;
			half _TS;
			half _TA;
			half _Res;
			int _Repeatedness;
			static const float tau = 6.2831853;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = {0.0,0.0,0.0,0.0};
				
				half2 uv = IN.texcoord;
				half2 z = { 0.5, 0.5 };
				uv -= z;
				uv *= 2.0; //centered
				uv = round(uv*_Res) / _Res;
				half2 o = { 0.0,0.0 };
				half d = distance(uv, o);

				half tf = _Thicc + _TA*sin(_TS*_Time*tau);

				fixed4 bcolor = lerp(_C2,_C1,((uv.x + uv.y) / 2.0)+0.5);

				c = lerp(c, bcolor, step(d, 1.0)*step(1.0 - tf, d)); //colored

				//time to clip

				half ang = -atan2(-uv.y, -uv.x);
				half start = ((_Time*_Speed)+_HoleO) % tau;

				start = lerp(start + tau, start, step(0.0, start));

				half end = (start + _HoleS) % tau;

				end = lerp(end + tau, end, step(0.0, end));

				ang = (lerp(ang + tau, ang, step(0.0, ang))*_Repeatedness)%tau; //normalizes the function to range (0,2pi)

				if (start > end)
				{
						clip(ang - end);
						clip(start-ang);
				}
				else
				{
				
					clip(abs(ang-((start+end)/2.0))-((end-start)/2.0)); //hmm
				}

				c *= _Color;
				c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}
