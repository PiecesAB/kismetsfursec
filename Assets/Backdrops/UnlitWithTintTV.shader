Shader "Unlit/UnlitWithTintTV"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_TVTex ("Texture", 2D) = "white" {}
		_TVReg("TV Texture Region (XY = bottom left, ZW = size)", Vector) = (0,0,1,1)
		_TVPos ("TV Pos (XY = bottom left, ZW = size)", Vector) = (0,0,1,1)
		_Darken ("Darken", Float) = 0.0
		_ScaleOffset ("Scale (XY), Offset (ZW)", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _TVTex;
			float4 _TVReg;
			float4 _TVPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed4 _Color;
			half4 _ScaleOffset;
			half _Darken;

            fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float2 mappy = frac((i.uv*_ScaleOffset.xy) + _ScaleOffset.zw);
				fixed4 col = tex2D(_MainTex, (i.uv*_ScaleOffset.xy) + _ScaleOffset.zw);
				
				if (mappy.x >= _TVPos.x && mappy.x <= _TVPos.x + _TVPos.z &&
					mappy.y >= _TVPos.y && mappy.y <= _TVPos.y + _TVPos.w) {
					float2 tcoord = float2(
						((mappy.x - _TVPos.x) / _TVPos.z) * _TVReg.z + _TVReg.x,
						((mappy.y - _TVPos.y) / _TVPos.w) * _TVReg.w + _TVReg.y
					);
					col = tex2D(_TVTex, tcoord);
				}

				col.rgb = lerp(col.rgb, 0.5 * lerp(col.rgb, 0.3 * col.r + 0.59 * col.g + 0.11 * col.b, 0.3), _Darken);
				col.rgba *= _Color.rgba;
				
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
