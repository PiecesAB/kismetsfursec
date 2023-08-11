// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/CircleBend"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Center("Center", Vector) = (0,0,0,0)
	}
		SubShader
	{
		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0

		// note: no SV_POSITION in this struct
		struct v2f {
		float2 uv : TEXCOORD0;
	};

	float4 UnityObjectToClipPos2(in float3 pos)
	{
#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
		// More efficient than computing M*VP matrix product
		return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#else
		return UnityObjectToClipPos(float4(pos, 1.0));
#endif
	}

	v2f vert(
		float4 vertex : POSITION, // vertex position input
		float2 uv : TEXCOORD0, // texture coordinate input
		out float4 outpos : SV_POSITION // clip space position output
	)
	{
		v2f o;
		o.uv = uv;
		outpos = UnityObjectToClipPos(vertex);
		return o;
	}

	sampler2D _MainTex;
	half4 _Center;
	static const float sqr2 = 1.414214;
	static const float sqr2_1 = 0.414214;

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
	{
		float dist = distance(i.uv,_Center.xy);

	fixed showCenter = step(dist,0.01);

		half angle = atan2(i.uv.y-_Center.y, i.uv.x - _Center.x);
		half h = sqr2 - (sqr2_1*abs(cos((2 * angle))));
		half2 newUv = _Center.xy;

		newUv.x += h*dist*cos(angle);
		newUv.y += h*dist*sin(angle);

		//clip(0.5 - abs(newUv.x - 0.5));
		//clip(0.5 - abs(newUv.y - 0.5));

		fixed4 lol = { 1,0,0,1 };
		fixed4 color = lerp(tex2D(_MainTex, newUv),lol,showCenter);

		return color;
	}
		ENDCG
	}
	}
}