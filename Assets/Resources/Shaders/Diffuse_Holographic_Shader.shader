Shader "Custom/Diffuse_Holographic_Shader" {
	Properties {
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		_EmissionTex("Emission Texture", 2D) = "white" {}
		_TextureAlbedo ("Diffuse Texture Multiplier", Color) = (1,1,1,1)
		_EmissionAlbedo("Emission Texture Multiplier", Color) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#include "HelperShaderFunctions.cginc"
		#pragma surface surf Lambert
		#pragma target 3.0
		

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
		};
		
		sampler2D _MainTex;
		sampler2D _EmissionTex;
		float4 _TextureAlbedo;
		float4 _EmissionAlbedo;

		//Uniforms updated by IslandViz
		float3 hologramCenter = float3(0, 0, -1.42f);
		float  hologramScale = 0.8f;
		float  hologramOutlineWidth = 0.5f;
		float3 hologramOutlineColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o)
		{
			//Clip fragments outside of the hologram center
			circleClip(IN.worldPos, hologramCenter.xyz, hologramScale);
			o.Albedo   = (tex2D(_MainTex, IN.uv_MainTex) * _TextureAlbedo).rgb;
			o.Emission = (tex2D(_EmissionTex, IN.uv_MainTex) * _EmissionAlbedo).rgb;

			//Hologram Outlines
			hologramCenter.y                                 = IN.worldPos.y;
			float dist                                       = distanceFromPoint(IN.worldPos, hologramCenter);
			float distFromBorder                             = hologramScale - dist;
			float scaledOutlineWidth                         = hologramOutlineWidth*hologramScale;
			float percentageDistanceFromBorderToOutlineWidth = max(0, (scaledOutlineWidth - distFromBorder) / scaledOutlineWidth);
			if (percentageDistanceFromBorderToOutlineWidth  <= 1.0f)
			{
				o.Emission = lerp(o.Emission, hologramOutlineColor, percentageDistanceFromBorderToOutlineWidth);
				o.Albedo  *= 1.0 - percentageDistanceFromBorderToOutlineWidth;
			}
		}

		ENDCG
	}
	FallBack "Diffuse"
}
