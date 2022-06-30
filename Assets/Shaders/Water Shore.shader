Shader "Custom/Water Shore" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard alpha SurfaceOutputStandard
		#pragma target 3.0

		// #include "Water.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float shore = IN.uv_MainTex.y;
			shore = sqrt(shore);
			// float foam = Foam(shore, IN.worldPos.xz, _MainTex);
			float2 noiseUV = IN.worldPos.xz + _Time.y * 0.25;
			float4 noise = tex2D(_MainTex, noiseUV * 0.015);
			float distortion1 = noise.x * (1 - shore);
			float foam1 = sin((shore + distortion1) * 10 - _Time.y);
			foam1 *= foam1;
			float distortion2 = noise.y * (1 - shore);
			float foam2 = sin((shore + distortion2) * 10 + _Time.y + 2);
			foam2 *= foam2 * 0.7;

			float foam = max(foam1, foam2) * shore;
			foam *= foam * shore;
			// float waves = Waves(IN.worldPos.xz, _MainTex);
			// waves *= 1 - shore;

			//fixed4 c = saturate(_Color + max(foam, waves));
			fixed4 c = saturate(_Color + foam);
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}