Shader "Noise/PerlinNoise3D" {
	Properties {
		_Frequency("Frequency", float) = 5
		_Octave("Octave(int)", int) = 1
		_Lacunarity("Lucanarity", float) = 2
		_Persistence("Persistence", float) = 0.5
		
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float _Frequency;
			int _Octave;
			float _Lacunarity, _Persistence;
			
			uniform sampler2D _hashTexture, _gradient3DTexture;

			struct vertexIn
			{
				float4 vertex: POSITION;
			};
			
			struct vertexOut
			{
				float4 pos: SV_POSITION;
				float3 hpoint: TEXCOORD;
			};
			
			vertexOut vert(vertexIn vi)
			{
				vertexOut vo;
				
				vo.pos = mul(UNITY_MATRIX_MVP, vi.vertex);
				vo.hpoint = vo.pos.xyz;
				
				return vo;
			}
			
			float3 fade(float3 t)
			{
				return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
			}
			
			float dotGradi(float u, float3 diffVec)
			{
				// convert from [0, 1] to [-1, 1]
				float3 g = tex2D(_gradient3DTexture, float2(u, 0)).xyz * 2.0 - 1.0;
				return dot(g, diffVec);
			}
			
			float noise(float3 p, float freq)
			{
				p *= freq;
				float3 i0 = floor(p);
				float3 t0 = p - i0;
				float3 t1 = t0 - 1;
				float3 i1 = (i0 + 1) / 256.0;	
				i0 /= 256.0;
				
				// no hash for i0.z now, it will be perfomed within 
				// gradient texel fetch thanks to how we store the gradients.
				// hi0.x = h000, hi0.y = h100, hi0.z = h010, hi0.w = h110
				// based on how we store hash table into the texture
				float4 hi = tex2D(_hashTexture, i0.xy);
				float4 hi0 = hi + i0.z;
				float4 hi1 = hi + i1.z;				
				
				float v000 = dotGradi(hi0.x, t0);
				float v100 = dotGradi(hi0.y, float3(t1.x, t0.y, t0.z));
				float v010 = dotGradi(hi0.z, float3(t0.x, t1.y, t0.z));
				float v110 = dotGradi(hi0.w, float3(t1.x, t1.y, t0.z));
				float v001 = dotGradi(hi1.x, float3(t0.x, t0.y, t1.z));
				float v101 = dotGradi(hi1.y, float3(t1.x, t0.y, t1.z));
				float v011 = dotGradi(hi1.z, float3(t0.x, t1.y, t1.z));
				float v111 = dotGradi(hi1.w, t1);
				
				float3 t = fade(t0);
				
				float r = lerp(
						lerp(lerp(v000, v100, t.x), lerp(v010, v110, t.x), t.y),
						lerp(lerp(v001, v101, t.x), lerp(v011, v111, t.x), t.y), t.z);
						
				return r;
			}
			
			float fractal(float3 p)
			{
				float sum = 0;
				float amplitude = 1.0;
				float range = 0;
				float freq = _Frequency;
				for (int i = 0; i < _Octave; ++i)
				{
					sum += noise(p, freq) * amplitude;
					range += amplitude;
					freq *= _Lacunarity;
					amplitude *= _Persistence;
				}
				
				return sum / range;
			}
			
			float4 frag(vertexOut vo): COLOR
			{
				float c = fractal(vo.hpoint) * 0.5 + 0.5;
				return float4(c, c, c, 1.0);
			} 
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
