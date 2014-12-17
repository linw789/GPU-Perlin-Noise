using UnityEngine;

public enum Dimension
{
	D1 = 1,
	D2 = 2,
	D3 = 3
}

public class TextureCreator: MonoBehaviour 
{
	[Range (2, 512)]
	public int m_resolution = 256;

	public int m_frequency = 10;

	[Range (1, 8)]
	public int m_octaves = 1;

	public float m_lucunarity = 2f;

	public float m_persistence = 0.5f;

	public Gradient m_coloring;

	public Dimension m_dimension = Dimension.D1;

	public FilterMode m_filterMode = FilterMode.Trilinear;

	Texture2D m_texture;

	void Awake()
	{
		m_texture = new Texture2D(m_resolution, m_resolution, TextureFormat.RGB24, true);
		m_texture.name = "Noise Texture";
		m_texture.wrapMode = TextureWrapMode.Clamp;
		m_texture.anisoLevel = 9;
		GetComponent<MeshRenderer>().material.mainTexture = m_texture;
	}

	// also gets called after re-compile
	void OnEnable()
	{
		FillTexture();
	}

	void Update()
	{
		if (transform.hasChanged)
		{
			transform.hasChanged = false;
			FillTexture();
		}
	}

	public void FillTexture()
	{
		m_texture.filterMode = m_filterMode;

		if (m_texture.width != m_resolution)
		{
			m_texture.Resize(m_resolution, m_resolution);
		}

		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

		float stepSize = 1f / m_resolution;
		for (int y = 0; y < m_resolution; ++y)
		{
			Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);

			for (int x = 0; x < m_resolution; ++x)
			{
				Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
				// perlin noise produces values between -1 and +1
				float sample = 0;
				switch ((int)m_dimension)
				{
				case 1:
					sample = Noise.Perlin1D(point.x, m_frequency);
					break;
				case 2:
					sample = Noise.Perlin2D(point, m_frequency);
					break;
				case 3:
					sample = Noise.PerlinFractal3D(point, m_frequency, 
					                               m_octaves, m_lucunarity, m_persistence);
					break;
				default:
					sample = Noise.Perlin1D(point.x, m_frequency);
					break; // must also add break in the last case
				}
				sample = sample * 0.5f + 0.5f;
				//m_texture.SetPixel(x, y, m_coloring.Evaluate(sample));
				m_texture.SetPixel(x, y, Color.white * sample);
			}
		}

		m_texture.Apply();
	}
}
