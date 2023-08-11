// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/CylinderBend"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_FColor("Fade Color", Color) = (1.0,1.0,1.0,1.0)
		_FInten("Fade Intensity", Range(-1.0,1.0)) = 30.0
		_VBend("Vertical (Y-Axis) Bend", Range (0.0,89.0)) = 30.0
		_HBend("Horizontal (X-Axis) Bend", Range(0.0,89.0)) = 0.0
		_Center("Center Position (X and Y)", Vector) = (0.5,0.5,0,0)
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
	fixed4 _FColor;
	fixed _FInten;
	fixed _HBend;
	fixed _VBend;
	half4 _Center;
	static const float degToRad = 0.017453*4;

	fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
	{

		half distX = i.uv.x-_Center.x;
		half distY = i.uv.y-_Center.y;

		half2 newUv = _Center.xy;

		half stretchX = 1.0 / cos(degToRad*max(min(_VBend*distY*0.5, 89.0),-89.0));
		half stretchY = 1.0 / cos(degToRad*max(min(_HBend*distX*0.5, 89.0),-89.0));

		newUv.x += distX/stretchX;
		newUv.y += distY/stretchY;

		clip(0.5-abs(newUv.x-0.5));
		clip(0.5 - abs(newUv.y - 0.5));

		fixed4 color = lerp(tex2D(_MainTex, newUv),_FColor,0);

		return color;
	}
		ENDCG
	}
	}
}