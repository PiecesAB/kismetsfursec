// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Water1"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tran("Liquid Transparency", Range(0.0,1.0)) = 0.5
		_Tint("Depths Tint", Color) = (1,1,1,1)
		_ST("Surface Tint", Color) = (1,1,1,1)
		_GS("Gradient Speed", Float) = 0.005
	    _DT("Distortion Size", Float) = 0.01
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
				float depth : TEXCOORD2;
				float4 halfsize : TEXCOORD3;
				float4 dif : TEXCOORD4;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Env;
			float _WS;
			float _GS;
			float _DT;
			
			v2f vert (appdata v)
			{
				v2f o;
				float4 fd = mul(unity_ObjectToWorld, v.vertex);
				float4 center = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));
				float4 dif = fd - center; // unrotated object position
				o.dif = dif;
				fd += float4(2.0 * sign(dif.x), 2.0 * sign(dif.y), 0.0, 0.0);
				o.halfsize = float4(abs(dif.x) + 2.0, abs(dif.y) + 2.0, 0.0, 0.0);
				o.halfsize.zw = 1.0 / o.halfsize.xy;
				o.vertex = mul(UNITY_MATRIX_VP, fd);
				//float ii = length(float3(_Object2World[0].y, _Object2World[1].y, _Object2World[2].y));
				float dpa = mul(unity_ObjectToWorld, float4(0.0,8.0,0.0,1.0)).y;
				o.depth = /*(-0.5/((0.005*(fd.y - dpa))-0.4))-1.0*/_GS*(fd.y - dpa)+0.2;
				o.grabpos = ComputeScreenPos(o.vertex);
				//o.grabpos.y = 1.0 - o.grabpos.y;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// these waves succ lol
				//o.vertex.y += _WS*o.uv.y*cos(_Time.y+(o.vertex.x*7.0));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed _Tran;
			fixed4 _Tint;
			fixed4 _ST;
			
			fixed4 frag (v2f i) : SV_Target
			{ 
				// make waves on edges by cutting out
				clip(i.dif.x + i.halfsize.x - 4.0 + 2.0 * sin(0.2 * i.dif.y + _Time.z * 3.0));
				clip(-i.dif.x + i.halfsize.x - 4.0 - 2.0 * sin(0.2 * i.dif.y + _Time.z * 3.0));
				clip(i.dif.y + i.halfsize.y - 4.0 + 2.0 * sin(0.2 * i.dif.x + _Time.z * 3.0));
				clip(-i.dif.y + i.halfsize.y - 4.0 - 2.0 * sin(0.2 * i.dif.x + _Time.z * 3.0));

				float2 uv = i.uv;
				//uv.xy = (floor(uv.xy*_ScreenParams.xy) / _ScreenParams.xy) + (0.5/_ScreenParams.xy);
				// sample the texture
				float2 gp2 = i.grabpos;
				gp2.x += _DT*sin(1.8*_Time.z+(i.vertex.y*0.02));
				gp2.y += _DT*cos(_Time.z-(i.vertex.x*0.02));
				gp2.xy = floor(gp2.xy*_ScreenParams.xy) / _ScreenParams.xy + (0.5/_ScreenParams.xy);
				fixed4 col = lerp(tex2D(_MainTex, i.uv),tex2D(_Env, gp2),_Tran);
				col *= _Tint;
				col = lerp(col, _ST, i.depth);
				//col.a = _Tint.a;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
