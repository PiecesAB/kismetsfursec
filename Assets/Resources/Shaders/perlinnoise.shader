// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PerlinNoiseDistortion"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Inten("Intensity", Range(0,0.3)) = 0.03
			_Speed("Speed", Float) = 0.1
			_Size("Size", Float) = 0.1
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

	float _Speed;
	float hash(float n)
	{
		return (sin(n + _Speed*_Time.z)*0.420);
	}

	float noize(float3 x)
	{

		float3 p = x/*floor(x)*/;
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*57.0 + 113.0*p.z;

		return lerp(lerp(lerp(hash(n + 6.0), hash(n + 7.0), f.z),
			lerp(hash(n + 63.0), hash(n + 62.0), f.z), f.y),
			lerp(lerp(hash(n + 113.0), hash(n + 112.0), f.z),
				lerp(hash(n + 171.0), hash(n + 172.0), f.z), f.y), f.x);
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
	float _Inten;
	
	float _Size;

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
	{
		float3 cool = {i.uv.x ,i.uv.y ,0};
		half2 add = {_Inten*(noize(_Size*cool)),_Inten*(noize(cool*_Size)) };
	half2 newUv = i.uv + add;
		fixed4 color = tex2D(_MainTex, newUv);

		return color;
	}
		ENDCG
	}
	}
}