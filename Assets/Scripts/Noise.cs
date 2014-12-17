using UnityEngine;
using System.Collections;

public static class Noise
{
	private static int[] m_hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,
		
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};

	const int m_hashMask = 255;
	const float m_oneOverHashMask = 1f / 255f;

	static float[] m_gradients1D = {-1f, 1f};
	const int m_gradient1DMask = 1;

	static Vector2[] m_gradients2D = {
		new Vector2( 1f, 0f),
		new Vector2(-1f, 0f),
		new Vector2( 0f, 1f),
		new Vector2( 0f,-1f),
		(new Vector2( 1f, 1f)).normalized,
		(new Vector2(-1f, 1f)).normalized,
		(new Vector2( 1f,-1f)).normalized,
		(new Vector2(-1f,-1f)).normalized
	};
	const int m_gradient2DMask = 7;

	static Vector3[] m_gradients3D = {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};
	const int m_gradient3DMask = 15;

	static float m_sqrt2 = Mathf.Sqrt(2f);

	// textures used in shaders to compute noise
	static Texture2D m_hashTexture2D, m_gradient3DTexture;

	public static Texture2D GetHashTexture2D()
	{
		return m_hashTexture2D;
	}

	public static Texture2D GetGradient3DTexture()
	{
		return m_gradient3DTexture;
	}

	public static void LoadResourceToTexture()
	{
		FillHashTexture2D();
		FillGradient3DTexture();
	}

	private static void SuffleHashTable(int seed)
	{
		Random.seed = seed;

		for (int i = 0; i <= m_hashMask; ++i)
		{
			int j = Random.Range(0, m_hashMask + 1);
			int tmp = m_hash[i];
			m_hash[i] = m_hash[j];
			m_hash[j] = tmp;
		}

		for (int i = 0; i <= m_hashMask; ++i)
		{
			m_hash[m_hashMask + 1 + i]  = m_hash[i];
		}
	}

	public static float Perlin1D(float point, float frequency)
	{
		point *= frequency;
		int i0 = Mathf.FloorToInt(point);

		// think of t0 and t1 as 1D vectors
		float t = point - i0;
		float t0 = t;
		float t1 = t0 - 1;

		i0 &= m_hashMask;
		int i1 = (i0 + 1) & m_hashMask;

		float g0 = m_gradients1D[m_hash[i0] & m_gradient1DMask];
		float g1 = m_gradients1D[m_hash[i1] & m_gradient1DMask];

		float v0 = g0 * t0;
		float v1 = g1 * t1;

		t = Fade(t);
		float r = Mathf.Lerp(v0, v1, t);

		// Had we do linear interpolation, 
		// r = v0 * (1 - t) + v1 * t = g0 * t0 * (1 - t) + g1 * t1 * t
		//   = g0 * t0 * (1 - t0) + g1 * (t0 - 1) * t0
		//   = (g0 - g1) * [-(t0 - 0.5)^2 + 0.25]
		// which has max_r at t = 0.5 .
		// Since (g0 - g1) ranges from -2 to 2, r rangs from -0.5 to 0.5
		// In order to get value from -1 to 1, we multiply r with 2
		return r * 2;
	}

	public static float Perlin2D(Vector2 point, float frequency)
	{
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);

		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;

		ix0 &= m_hashMask;
		iy0 &= m_hashMask;
		int ix1 = (ix0 + 1) & m_hashMask;
		int iy1 = (iy0 + 1) & m_hashMask;


		Vector2 g00 = m_gradients2D[m_hash[(m_hash[ix0] + iy0)] & m_gradient2DMask];
		Vector2 g10 = m_gradients2D[m_hash[(m_hash[ix1] + iy0)] & m_gradient2DMask];
		Vector2 g01 = m_gradients2D[m_hash[(m_hash[ix0] + iy1)] & m_gradient2DMask];
		Vector2 g11 = m_gradients2D[m_hash[(m_hash[ix1] + iy1)] & m_gradient2DMask];

		// 4 difference vectors each starts from one of the 4 points of a grid
		// and end at the point in question
		Vector2 vec00 = new Vector2(tx0, ty0);
		Vector2 vec10 = new Vector2(tx1, ty0);
		Vector2 vec01 = new Vector2(tx0, ty1);
		Vector2 vec11 = new Vector2(tx1, ty1);

		float v00 = Vector2.Dot(g00, vec00);
		float v10 = Vector2.Dot(g10, vec10);
		float v01 = Vector2.Dot(g01, vec01);
		float v11 = Vector2.Dot(g11, vec11);

		float tx = Fade(tx0);
		float ty = Fade(ty0);

		float r = Mathf.Lerp(
			Mathf.Lerp(v00, v10, tx),
			Mathf.Lerp(v01, v11, tx), ty);

		return r * m_sqrt2;
	}

	public static float Perlin3D(Vector3 point, float frequency)
	{
		point *= frequency;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);

		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tz0 = point.z - iz0;
		float tx1 = tx0 - 1;
		float ty1 = ty0 - 1;
		float tz1 = tz0 - 1;

		ix0 &= m_hashMask;
		iy0 &= m_hashMask;
		iz0 &= m_hashMask;
		int ix1 = (ix0 + 1) & m_hashMask;
		int iy1 = (iy0 + 1) & m_hashMask;
		int iz1 = (iz0 + 1) & m_hashMask;

		int h00 = m_hash[m_hash[ix0] + iy0];
		int h10 = m_hash[m_hash[ix1] + iy0];
		int h01 = m_hash[m_hash[ix0] + iy1];
		int h11 = m_hash[m_hash[ix1] + iy1];
		Vector3 g000 = m_gradients3D[m_hash[h00 + iz0] & m_gradient3DMask];
		Vector3 g100 = m_gradients3D[m_hash[h10 + iz0] & m_gradient3DMask];
		Vector3 g010 = m_gradients3D[m_hash[h01 + iz0] & m_gradient3DMask];
		Vector3 g110 = m_gradients3D[m_hash[h11 + iz0] & m_gradient3DMask];
		Vector3 g001 = m_gradients3D[m_hash[h00 + iz1] & m_gradient3DMask];
		Vector3 g101 = m_gradients3D[m_hash[h10 + iz1] & m_gradient3DMask];
		Vector3 g011 = m_gradients3D[m_hash[h01 + iz1] & m_gradient3DMask];
		Vector3 g111 = m_gradients3D[m_hash[h11 + iz1] & m_gradient3DMask];

		float v000 = Vector3.Dot(g000, new Vector3(tx0, ty0, tz0));
		float v100 = Vector3.Dot(g100, new Vector3(tx1, ty0, tz0));
		float v010 = Vector3.Dot(g010, new Vector3(tx0, ty1, tz0));
		float v110 = Vector3.Dot(g110, new Vector3(tx1, ty1, tz0));
		float v001 = Vector3.Dot(g001, new Vector3(tx0, ty0, tz1));
		float v101 = Vector3.Dot(g101, new Vector3(tx1, ty0, tz1));
		float v011 = Vector3.Dot(g011, new Vector3(tx0, ty1, tz1));
		float v111 = Vector3.Dot(g111, new Vector3(tx1, ty1, tz1));

		float tx = Fade(tx0);
		float ty = Fade(ty0);
		float tz = Fade(tz0);

		float r = Mathf.Lerp(
			Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty),
			Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty), tz);

		return r;
	}

	public static float PerlinFractal1D(float point, float frequency, 
	                                   int octaves, float lacunarity = 2, float persistence = .5f)
	{
		float sum = 0;
		float amplitude = 1f;
		float range = 0;
		for (int o = 0; o < octaves; o++) 
		{
			sum += Perlin1D(point, frequency) * amplitude;
			range += amplitude;
			frequency *= lacunarity;
			amplitude *= persistence;
		}
		return sum / range;
	}

	public static float PerlinFractal2D(Vector2 point, float frequency, 
	                                    int octaves, float lacunarity = 2, float persistence = .5f)
	{
		float sum = 0;
		float amplitude = 1f;
		float range = 0;
		for (int o = 0; o < octaves; o++) 
		{
			sum += Perlin2D(point, frequency) * amplitude;
			range += amplitude;
			frequency *= lacunarity;
			amplitude *= persistence;
		}
		return sum / range;
	}

	public static float PerlinFractal3D(Vector3 point, float frequency, 
	                                    int octaves, float lacunarity = 2, float persistence = .5f)
	{
		float sum = 0;
		float amplitude = 1f;
		float range = 0;
		for (int o = 0; o < octaves; o++) 
		{
			sum += Perlin3D(point, frequency) * amplitude;
			range += amplitude;
			frequency *= lacunarity;
			amplitude *= persistence;
		}
		return sum / range;
	}

	private static void FillHashTexture2D()
	{
		if (m_hashTexture2D)
			return;

		m_hashTexture2D = new Texture2D(m_hashMask + 1, m_hashMask + 1, TextureFormat.ARGB32, false, true);
		m_hashTexture2D.name = "Noise Hash Texture";
		// fetch texel at exact coordinate without any blending
		m_hashTexture2D.filterMode = FilterMode.Point;
		// so index out of bounds will still work, also not bit mask operation needed
		m_hashTexture2D.wrapMode = TextureWrapMode.Repeat;

		float fctr = 1f / 255f;
		for (int y = 0; y <= m_hashMask; ++y) 
		{
			for (int x = 0; x <= m_hashMask; ++x)
			{
//				int h0y = m_hash[x] + y;
//				int h1y = m_hash[x + 1] + y;
				int h00 = m_hash[m_hash[x] + y];
				int h10 = m_hash[m_hash[x + 1] + y];
				int h01 = m_hash[m_hash[x] + y + 1];
				int h11 = m_hash[m_hash[x + 1] + y + 1];

				m_hashTexture2D.SetPixel(x, y, new Color(h00 * fctr, h10 * fctr, h01 * fctr, h11 * fctr));
			}
		}

		m_hashTexture2D.Apply();
	}

	private static void FillGradient3DTexture()
	{
		if (m_gradient3DTexture)
			return;

		m_gradient3DTexture = new Texture2D(m_hashMask + 1, 1, TextureFormat.RGB24, false, true);
		m_gradient3DTexture.name = "Gradient3D Texture";
		m_gradient3DTexture.filterMode = FilterMode.Point;
		m_gradient3DTexture.wrapMode = TextureWrapMode.Repeat;

		// hash the index then get the gradient
		for (int i = 0; i <= m_hashMask; ++i)
		{
			int k = m_hash[i] & 15;
			// convert [-1, 1] to [0, 1]
			float x = m_gradients3D[k].x * 0.5f + 0.5f;
			float y = m_gradients3D[k].y * 0.5f + 0.5f;
			float z = m_gradients3D[k].z * 0.5f + 0.5f;

			m_gradient3DTexture.SetPixel(i, 0, new Color(x, y, z));
		}

		m_gradient3DTexture.Apply();
	}

	private static float Fade(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}
}
