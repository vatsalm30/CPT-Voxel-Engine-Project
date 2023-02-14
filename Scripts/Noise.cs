using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public float perlin (float x, float z, int octaves, float persistence) {
		float total = 0;
		float frequency = 1;
		float amplitude = 1;
		for (int i=0;i<octaves;i++) {
			total += Mathf.PerlinNoise(x* frequency, z* frequency) * amplitude;
			
			amplitude *= persistence;
			frequency *= 2;
		}
		return total;
	}

    public float ThreeDNoise(float x, float y, float z, float scale)
    {
        float xy = Mathf.PerlinNoise(x * scale, y * scale);
        float xz = Mathf.PerlinNoise(x * scale, z * scale);
        float zy = Mathf.PerlinNoise(z * scale, y * scale);

        float yx = Mathf.PerlinNoise(y * scale, x * scale);
        float zx = Mathf.PerlinNoise(z * scale, x * scale);
        float yz = Mathf.PerlinNoise(z * scale, y * scale);

        float xyz = xy + xz + yz + yx + zx + yz + zy;
        return xyz / 6f;
    }

}
