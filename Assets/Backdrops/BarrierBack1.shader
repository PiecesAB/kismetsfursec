Shader "Unlit/Barrier1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_SubTex("Subtexture", 2D) = "white" {}
		_Color1("Color1", Color) = (1,1,1,1)
		_Color2("Color2", Color) = (1,1,1,1)
		_ScrollSpeed("Scroll Speed", Float) = 10.0
		_FakeZ("Fake Perspective", Vector) = (0,0,0,0)
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
        LOD 100
		Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color    : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _SubTex;
			float4 _SubTex_ST;
			fixed4 _Color1;
			fixed4 _Color2;
			float4 _FakeZ;

            v2f vert (appdata v)
            {
				float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
				float4 p = mul(UNITY_MATRIX_MV, v.vertex);
				if (any(_FakeZ.xy))
				{
					float mult = ((_FakeZ.z - _FakeZ.w) / (posWorld.z - _FakeZ.w));
					p.x *= (1.0 - _FakeZ.x) + (_FakeZ.x*mult);
					p.y *= (1.0 - _FakeZ.y) + (_FakeZ.y*mult);
				}
				v2f o;
				o.vertex = mul(UNITY_MATRIX_P, p);
				o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _SubTex);
                return o;
            }

			float _ScrollSpeed;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 c = tex2D(_MainTex, i.uv + float2(0.0, - _ScrollSpeed* _Time.x));
				fixed4 cr = c;
				//cr.rgb = 0.0;
				fixed4 c1 = _Color1 * tex2D(_SubTex, float2(i.uv2.x - round(_Time.x * 6.0) / 20.0, round(_Time.x * 150.0) / 160.0 + (i.uv2.y * 0.125)));
				fixed4 c2 = _Color2 * tex2D(_SubTex, float2(i.uv2.x + round(_Time.x * 5.0) / 20.0, round(-_Time.x * 90.0) / 160.0 + (i.uv2.y * 0.125)));
				clip(c1.a + c2.a - 0.1);
				cr = c * c1 * c1.a + c * c2 * c2.a;
				c.a *= max(c1.a, c2.a);
				cr.rgb *= c.a;
				//cr.rgb *= max(c1.a, c2.a);
                return cr;
            }
            ENDCG
        }
    }
}
