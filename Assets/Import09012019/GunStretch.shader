Shader "Sprites/GunStretch"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_H("Horizontal Stretch", Float) = 0.05
		_V("Horizontal Stretch", Float) = 0.015
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _H;
			half _V;
			
			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.xy += 0.6*sin(0.12*(v.vertex.x+v.vertex.y-_Time.z));
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.y += _V*cos(1.5*(_Time.z + i.uv.x*20.0));
				i.uv.x += _H*cos(1.4*(_Time.z + i.uv.y*19.0));
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
