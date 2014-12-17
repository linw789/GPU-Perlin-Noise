using UnityEngine;
using System.Collections;

public class GPU_noise_test : MonoBehaviour 
{
	void Start()
	{
		Noise.LoadResourceToTexture();

		renderer.material.SetTexture("_hashTexture", Noise.GetHashTexture2D());
		renderer.material.SetTexture("_gradient3DTexture", Noise.GetGradient3DTexture());
	}
}
