Shader "Custom/Estuaries" {
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
		#pragma surface surf Standard alpha vertex:vert
		#pragma target 3.0

		// #include "Water.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float2 riverUV;
			float3 worldPos;
		};
		float Foam (float shore, float2 worldXZ, sampler2D noiseTex) {
			shore = sqrt(shore)* 0.9;

			float2 noiseUV = worldXZ + _Time.y * 0.25;
			float4 noise = tex2D(noiseTex, noiseUV * 0.015);

			float distortion1 = noise.x * (1 - shore);
			float foam1 = sin((shore + distortion1) * 10 - _Time.y);
			foam1 *= foam1;

			float distortion2 = noise.y * (1 - shore);
			float foam2 = sin((shore + distortion2) * 10 + _Time.y + 2);
			foam2 *= foam2 * 0.7;

			return max(foam1, foam2) * shore;
		}

		float Waves (float2 worldXZ, sampler2D noiseTex) {
			float2 uv1 = worldXZ;
			uv1.y += _Time.y;
			float4 noise1 = tex2D(noiseTex, uv1 * 0.025);

			float2 uv2 = worldXZ;
			uv2.x += _Time.y;
			float4 noise2 = tex2D(noiseTex, uv2 * 0.025);

			float blendWave = sin(
				(worldXZ.x + worldXZ.y) * 0.1 +
				(noise1.y + noise2.z) + _Time.y
			);
			blendWave *= blendWave;

			float waves =
				lerp(noise1.z, noise1.w, blendWave) +
				lerp(noise2.x, noise2.y, blendWave);
			return smoothstep(0.75, 2, waves);
		}
		 float River (float2 riverUV, sampler2D noiseTex) {
            float2 uv = riverUV;
            uv.x = uv.x * 0.0625 + _Time.y * 0.005;
            uv.y -= _Time.y * 0.25;
            float4 noise = tex2D(noiseTex, uv);

            float2 uv2 = riverUV;
            uv2.x = uv2.x * 0.0625 - _Time.y * 0.0052;
            uv2.y -= _Time.y * 0.23;
            float4 noise2 = tex2D(noiseTex, uv2);
            
            return noise.x * noise2.w;
        }
		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.riverUV = v.texcoord1.xy;
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float shore = IN.uv_MainTex.y;
			float foam = Foam(shore, IN.worldPos.xz, _MainTex);
			float waves = Waves(IN.worldPos.xz, _MainTex);
			waves *= 1 - shore;
			float shoreWater = max(foam, waves);
			float river = River(IN.riverUV, _MainTex);
			float water = lerp(shoreWater, river, IN.uv_MainTex.x);
			fixed4 c = saturate(_Color + water);
			o.Albedo = c.rgb; 
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}