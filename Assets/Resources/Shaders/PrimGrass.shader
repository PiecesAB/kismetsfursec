// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Sprites/PrimGrass"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert nofog keepalpha
		#pragma multi_compile _ PIXELSNAP_ON
		#pragma shader_feature ETC1_EXTERNAL_ALPHA
		#pragma noambient
		#pragma nolighting
		//#pragma halfasview

		sampler2D _MainTex;
		fixed4 _Color;
		sampler2D _AlphaTex;


		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
			float worldq;
			float2 cp;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
			#if defined(PIXELSNAP_ON)
			v.vertex = UnityPixelSnap (v.vertex);
			#endif
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
			o.worldq = (v.vertex.x/480.0)-(v.vertex.z / 480.0);
			o.cp = mul(unity_ObjectToWorld, v.vertex).xz;
		}

		fixed4 SampleSpriteTexture (float2 uv)
		{
			fixed4 color = tex2D (_MainTex, uv);

			return color;
		}




		fixed3 ColCalc(half xb)
		{
			half ab = frac(xb*6.0);
			fixed3 ret = fixed3(1.0, ab, 0.0);
			if (xb > 0.8333) { ret = fixed3(1.0, 0.0, 1.0 - ab); }
			else if (xb > 0.6666) { ret = fixed3(ab, 0.0, 1.0); }
			else if (xb > 0.5) { ret = fixed3(0.0, 1.0 - ab, 1.0); }
			else if (xb > 0.3333) { ret = fixed3(0.0, 1.0, ab); }
			else if (xb > 0.1666) { ret = fixed3(1.0 - ab, 1.0, 0.0); }
			return ret * 0.3;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			//oh god

			//fixed4 c = IN.color;
			half a = atan2(IN.cp.y, IN.cp.x) * 0.15915494;
			IN.cp.xy *= 0.00125;
			a += 0.25 * abs(sin(IN.cp.x + IN.cp.y + _Time.z *0.1666666));//placeholder for noise
			half xb = (a + sin(_Time.z*0.2));
			fixed4 c = fixed4(ColCalc(xb - floor(xb)),0.0);


			float hd = mul(unity_WorldToObject, half4(IN.uv_MainTex,0.0,0.0)).x + _Time.z;
			half xp = 0.1*(sin((3.0*IN.uv_MainTex.y+IN.worldq) +hd) - sin(hd + IN.worldq));
			half yp = max(0.0, 0.1 - (IN.uv_MainTex.y*0.1));
			half zd = IN.uv_MainTex.x - xp - 0.5;
			half2 what = { 0.5,0.5 };
			c.a = max(0.0,min((yp - abs(zd))*70.0,1.0));
			//c.a = step(0.0,(yp - abs(zd))*70.0);
			clip(0.48 - distance(what, IN.uv_MainTex));
			half nc = 0.35*(zd / yp);
			half3 n3 = { nc,nc,nc * 0.5};
			o.Albedo = (c.rgb - n3) * c.a;
			o.Alpha = c.a;
		}
		ENDCG
	}

//Fallback "Transparent/VertexLit"
}
