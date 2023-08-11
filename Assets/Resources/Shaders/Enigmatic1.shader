// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Enigmatic1"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_ACol1("Active Color 1", Color) = (0.8,0.8,0.8,1.0)
		_ACol2("Active Color 2", Color) = (0.88,0.88,0.88,1.0)
		_BCol1("Background Color 1", Color) = (0.1,0.1,0.1,1.0)
		_BCol2("Background Color 2", Color) = (0.25,0.25,0.25,1.0)
		_PCol1("Passive Color 1", Color) = (0.3,0.3,0.6,1.0)
		_PCol2("Passive Color 2", Color) = (0.4,0.4,0.8,1.0)
		_WavInten("Wave Intensity", Range(0.0,1.0)) = 0.25
		_TexSize("Texture Size (X and Y only)", Vector) = (320.0,240.0,0.0,0.0)
			_CenterX("Center X", Range(0.0,1.0)) = 0.5
			_CenterY("Center Y", Range(0.0,1.0)) = 0.5
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
	fixed4 _ACol1;
	fixed4 _ACol2;
	fixed4 _BCol1;
	fixed4 _BCol2;
	fixed4 _PCol1;
	fixed4 _PCol2;
	float _CenterX;
	float _CenterY;
	float _WavInten;
	half4 _TexSize;
	#define pi 3.1415927
	

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
	{
float pifrac = pi*_WavInten;

	float2 lol = i.uv;
	float2 cool = { _CenterX, _CenterY };
	lol -= cool;
	lol *= _TexSize;

	float x = lol.x;
	float y = lol.y;
	float t = _Time.z*1.5;

	float pxt = pi*x + t;
	float cospxt = cos(pxt);
	float sinpxt = sin(pxt);
	float sin4pxt = sin((pifrac*x) + t);
	float cos4pxt = cos((pifrac*x) + t);
	float abpcpxt = abs(pi*cospxt);
	float abpspxt = abs(pi*sinpxt);

	float bigFunc = abs((sign(cospxt)*sinpxt) + sin4pxt) - ((3 / 64)*(abpcpxt*abpcpxt)) - y;
	float passiveFunc = abs((sign(-sinpxt)*cospxt) + cos4pxt) - ((3 / 64)*(abpspxt*abpspxt)) - y;
	float passCheck = step(passiveFunc, 1.0);
	float passCheck2 = step(passiveFunc, 0.4);
	float bigCheck = step(bigFunc, 1.0);
	float bigCheck2 = step(bigFunc, 0.3);

	fixed4 color = lerp(_BCol1, _BCol2, step(frac(lol.x+lol.y+_Time.y),0.5));
	color = lerp(color, _PCol1, passCheck);
	color = lerp(color, _PCol2, passCheck2);
	color = lerp(color,_ACol1,bigCheck);
	color = lerp(color, _ACol2, bigCheck2);

	fixed4 hmm = color - _ACol2;
	if (!any(hmm))
	{
		color = lerp(color, _ACol1, step(frac(lol.x + lol.y - _Time.y), 0.5));
	}

		return color;
	}
		ENDCG
	}
	}
}