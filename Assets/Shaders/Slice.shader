// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Clip" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	[NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
    _factor ("factor",Float) = -1.0
    _threshold ("threshold",Float) = 0.0
	_maxHeight ("Max Height", Float) = 0.006
	_Color ("Color", Color) = (1,1,1,1)
	_Glow ("Intensity", Range(0, 3)) = 1
	
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250
	Cull Off

	CGPROGRAM
	#pragma surface surf Lambert noforwardadd vertex:vert
	#pragma target 3.0

	sampler2D _MainTex;
	sampler2D _BumpMap;
	float _factor, _threshold, _maxHeight;
	fixed4 _Color;
    half _Glow;

	struct Input {
		float2 uv_MainTex;
		float3 localPos;
	};

	void vert (inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		o.localPos = v.vertex.xyz;
	}

    

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

		float f = _factor * ( IN.localPos.y / (_maxHeight * 100)) + _threshold;

		clip(f);
		if (f < 0.05) {
			o.Albedo = c.rgb * _Glow * _Color * (1 + (1-f / 0.05));
		} else {
			o.Albedo = c.rgb * _Color * _Glow;
		}
		o.Alpha = c.a;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	}


	ENDCG  
}

FallBack "Mobile/Diffuse"
}
