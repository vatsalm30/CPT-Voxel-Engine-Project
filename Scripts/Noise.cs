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
        float xy = perlin(x * scale, y * scale, 2, .5f);
        float xz = perlin(x * scale, z * scale, 2, .5f);
        float zy = perlin(z * scale, y * scale, 2, .5f);

        float yx = perlin(y * scale, x * scale, 2, .5f);
        float zx = perlin(z * scale, x * scale, 2, .5f);
        float yz = perlin(z * scale, y * scale, 2, .5f);

        float xyz = xy + xz + yz + yx + zx + yz + zy;
        return xyz / 6f;
    }

}
