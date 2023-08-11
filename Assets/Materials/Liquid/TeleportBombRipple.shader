// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/TeleportBombRipple"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RippleColor("Ripple Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Tran("Transparency", Range(0.0,1.0)) = 0.5
		_RT("Ripple Intensity", Float) = 1.0
		_RF("Ripple Frequency", Float) = 1.0
		_ROV("Oscillation Velocity", Float) = 1.0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		LOD 200
		Blend One OneMinusSrcAlpha

		GrabPass
		{
			"_Env"
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			// make fog work
			#pragma multi_compile_fog
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 grabpos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Env;

			v2f vert (appdata v)
			{
				v2f o;
				float4 fd = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(UNITY_MATRIX_VP, fd);
				//float ii = length(float3(_Object2World[0].y, _Object2World[1].y, _Object2World[2].y));
				o.grabpos = ComputeScreenPos(o.vertex);
				//o.grabpos.y = 1.0 - o.grabpos.y;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed _Tran;
			half _RT;
			half _RF;
			half _ROV;
			fixed4 _RippleColor;
			//fixed4 _Tint;
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				float2 uv = i.uv;
				float2 distFromCenter = distance(i.uv, float2(0.5, 0.5));
				float edgeSoftenMultiplier = clamp(5.0*(0.5 - distFromCenter), 0, 1);
				clip(0.5 - distFromCenter);
				float2 dir = normalize(i.uv - float2(0.5, 0.5));
				//uv.xy = (floor(uv.xy*_ScreenParams.xy) / _ScreenParams.xy) + (0.5/_ScreenParams.xy);
				// sample the texture
				float2 gp2 = i.grabpos;
				float coeff = _RT * sin(-3.6*_Time.z*_ROV +(distFromCenter*30.0*_RF));
				gp2 += 0.02 * edgeSoftenMultiplier * coeff*dir;
				gp2.xy = floor(gp2.xy*_ScreenParams.xy) / _ScreenParams.xy + (0.5/_ScreenParams.xy);
				
				fixed4 col = lerp(tex2D(_MainTex, i.uv),tex2D(_Env, gp2),_Tran);
				col -= 0.2* edgeSoftenMultiplier  * coeff * _RippleColor;
				//col *= _Tint;
				//col.a = _Tint.a;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
