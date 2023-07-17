using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Noise
{
    /*
     * perlin function used to get noise
     * param x is the x coord of the block
     * param z is the z coord of the block
     * octaves is how many layers of noise will be combined
     * persistence is how much each octave layer impacts the amplitude
    */
    public float perlin (float x, float z, int octaves, float persistence) {
		float total = 0;
		float frequency = 1;
		float amplitude = 1;
		for (int i=0;i<octaves;i++) {
			total += Mathf.PerlinNoise(x* frequency, z* frequency) * amplitude;
			
			amplitude *= persistence;
			frequency *= 2;
		}
		return total/octaves;
	}

    /*
     * perlin function used to get noise
     * param x is the x coord of the block
     * param y is the y coord of the block     
     * param z is the z coord of the block
     * scale is how gradual the changes will be (high scale means less gradual changes)
     * octaves is how many layers of noise will be combined
     * persistence is how much each octave layer impacts the amplitude
    */
    public float ThreeDNoise(float x, float y, float z, float scale, int octaves, float persistence)
    {
        float xy = perlin(x * scale, y * scale, octaves, persistence);
        float xz = perlin(x * scale, z * scale, octaves, persistence);
        float zy = perlin(z * scale, y * scale, octaves, persistence);

        float yx = perlin(y * scale, x * scale, octaves, persistence);
        float zx = perlin(z * scale, x * scale, octaves, persistence);
        float yz = perlin(z * scale, y * scale, octaves, persistence);

        float xyz = xy + xz + yz + yx + zx + yz + zy;
        return xyz / 6f;
    }

}
