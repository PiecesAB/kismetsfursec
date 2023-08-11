Shader "Unlit/UnlitWithTint"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
	    _Heatstroke ("Heatstroke effect (1 = ON)",Float) = 0.0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			fixed4 _Color;
			fixed _Heatstroke;
			half4 _ScaleOffset;

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, (i.uv*_ScaleOffset.xy) + _ScaleOffset.zw);

			    if (_Heatstroke > 0.5) {
					col = fixed4(col.b, col.g, col.r, col.a);
				}

				col.rgba *= _Color.rgba;
				
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
