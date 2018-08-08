Shader "Unlit/WireframeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureAlbedo("Texture Multiplier", Color) = (1,1,1,1)
		_WireThickness("Wire Thickness", Range (0,1)) = 0.1
	}
	SubShader
	{
			
		Tags { "RenderType"="Opaque"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex   vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float4 pos : POSITION;
				float2 uv  : TEXCOORD0;
			};

			struct g2f
			{
				float2 uv           : TEXCOORD0;
				float3 barycentric  : TEXCOORD1;
				float4 pos          : POSITION;
			};

			sampler2D _MainTex;
			float4    _MainTex_ST;
			float4    _TextureAlbedo;
			float     _WireThickness;
			
			//Vertex-Shader
			v2g vert (appdata v)
			{
				v2g o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			//Geometry-Shader
			[maxvertexcount(3)]
			void geom(triangle v2g v[3], inout TriangleStream<g2f> triStream)
			{
				g2f o;

				float3 a = v[1].pos - v[0].pos;
				float3 b = v[2].pos - v[0].pos;

				o.uv          = v[0].uv;
				o.barycentric = float3(1, 0, 0);
				o.pos         = v[0].pos;
				triStream.Append(o);

				o.uv = v[1].uv;
				o.barycentric = float3(0, 1, 0);
				o.pos = v[1].pos;
				triStream.Append(o);

				o.uv = v[2].uv;
				o.barycentric = float3(0, 0, 1);
				o.pos = v[2].pos;
				triStream.Append(o);
			
			}
			
			//Fragment-Shader
			fixed4 frag (g2f i) : SV_Target
			{
				float cutOff    = _WireThickness / 3.0;
				float minBary   = min(min(i.barycentric.x, i.barycentric.y), i.barycentric.z);
				//float smoothing = i.area*abs(ddx(minBary) + ddy(minBary));
				if (minBary >= cutOff)
					discard;

				fixed4 col    = tex2D(_MainTex, i.uv) * _TextureAlbedo;
				return col;
			}
			ENDCG
		}
	}
}
