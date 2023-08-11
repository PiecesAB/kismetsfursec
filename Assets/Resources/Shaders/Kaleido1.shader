Shader "Unlit/Kaliedo1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 col = fixed4(0.0,0.0,0.0,1.0);
				float2 coord = float2((i.uv.x - 0.5)*2.0, (i.uv.y - 0.5)*2.0);
				float a = _Time.x*2.0;
				float th = atan2(coord.y, coord.x);
				float r = distance(coord, float2(0.0, 0.0));
				float s1 = sqrt(abs(1.5*sin(4.0*(2.0*(th-r) - a)) + sin(a)));
				float s2 = abs(0.8*sin(12.0*((th+(1.4*r)) + a)) + sin(-a));
				float s3 = abs(tan(5.0*sin(a) + 8.0*th) -2.0 + 4.0*sin(6.0*a));
				float s4 = abs((0.4 + 0.2*sin(4.0*a)*sin(12.0*(th+10.0*r)))*sin(0.4*a)*2.5);
				if (r <= s1)
				{
					col += fixed4(0.2, 0.06, 0.0, 1.0);
				}
				if (r <= s2)
				{
					col += fixed4(0.04, 0.04, 0.04, 1.0);
					col *= 1.5;
				}
				if (r <= s3)
				{
					col += fixed4(0.0, 0.14, 0.08, 1.0);
				}
				if (r <= s4 && r >= s4*0.5)
				{
					col = lerp(fixed4(0.0,0.0,1.0,1.0),col,0.8);
					col += fixed4(0.0, 0.08, 0.2, 1.0);
				}
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
