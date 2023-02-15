using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockTexture[] blockTexture;
    public ThreeDNoiseFeatures[] threeDNoiseFeatures;
    public Transform player;
    public int ChunkHeight;
    public int ChunkWidth;

    public float scale;
    public int octaves;
    public float persistane;

    public float chunkReducer;

    public int RenderDist;

    List<int[]> chunksMatrix;
    void Start()
    {
        chunksMatrix = new List<int[]>();
        for (int x = 0; x < RenderDist; x++)
        {

            for (int z = 0; z < RenderDist; z++)
            {
                Chunk chunk = ChunkCreate(x, z);
                chunksMatrix.Add(new int[] {x,z});
                print(chunksMatrix[x][1]);
            }
        }
        
    }

    Chunk ChunkCreate(int xpos, int zpos)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Chunk [{xpos}, {zpos}]";
        gameObject.transform.SetParent(transform);
        gameObject.transform.position = new Vector3(xpos * ChunkWidth, 0, zpos * ChunkWidth);
        gameObject.layer = 3;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Chunk chunk = gameObject.AddComponent<Chunk>();

        chunk.xPos = xpos;
        chunk.zPos = zpos;

        meshRenderer.material = material;
        chunk.blockTexture = blockTexture;

        chunk.meshFilter = meshFilter;
        chunk.meshRenderer = meshRenderer;

        chunk.ChunkHeight = ChunkHeight;
        chunk.ChunkWidth = ChunkWidth;

        chunk.scale = scale;
        chunk.octaves = octaves;
        chunk.persistane = persistane;

        chunk.ChunkReducer = chunkReducer;
        chunk.xOffset = xpos * ChunkWidth;
        chunk.zOffset = zpos * ChunkWidth;

        chunk.threeDNoiseFeatures = threeDNoiseFeatures;

        return chunk;
    }
}

[System.Serializable]
public class BlockTexture
{
    public string blockName;
    public bool isSolid;
    [Header("Texture Values")]
    public int topTexture;
    public int bottomTexture;
    public int leftTexture;
    public int rightTexture;
    public int frontTexture;
    public int backTexture;

    public int GetTexture(int number)
    {
        switch (number)
        {
            case 0:
                return backTexture;
            case 1:
                return frontTexture;
            case 2:
                return topTexture;
            case 3:
                return bottomTexture;
            case 4:
                return leftTexture;
            case 5:
                return rightTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;

        }
    }
}


[System.Serializable]
public class ThreeDNoiseFeatures
{
    public string featureName;

    public float ThreeDScale;
    public float ThreeDThreshold;

    public int BlockID;
    public int GenerationMaxHeight;
    public int GenerationMinHeight;

    public Vector3 offset;

    Noise noise = new Noise();

    public int ThreeDBlockToPlace(Vector3 pos, int currentBlockId)
    {
        float num = noise.ThreeDNoise(pos.x + offset.x, pos.y + offset.y, pos.z + offset.z, ThreeDScale);
        if (num > ThreeDThreshold && pos.y > GenerationMinHeight && pos.y < GenerationMaxHeight)
            return BlockID;
        return currentBlockId;
    }

}