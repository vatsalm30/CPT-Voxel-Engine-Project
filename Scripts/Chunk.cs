using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    int[,] voxelTris = new int[6, 4]
    {
        {0, 3, 1, 2},
        {5, 6, 4, 7},
        {3, 7, 2, 6},
        {1, 5, 0, 4},
        {4, 7, 0, 3},
        {1, 2, 5, 6}
    };

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public BlockTexture[] blockTexture;
    public int ChunkHeight;
    public int ChunkWidth;
    bool[,,] ChunkData;
    public float scale;
    public int octaves;
    public float persistane;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int textureAtlasSize = 16;
    int vertexIndex = 0;
    public float xOffset;
    public float zOffset;
    public int xPos;
    public int zPos;
    public float ChunkReducer;
    public ThreeDNoiseFeatures[] threeDNoiseFeatures;

    float NormalizedBlockTextureSize
    {

        get { return 1f / (float)textureAtlasSize; }

    }


    Noise noise;

    void Start()
    {
        noise = new Noise();
        ChunkData = new bool[ChunkWidth,ChunkHeight, ChunkWidth];
        BlockCheck();
        CreatChunk(ChunkData);
        CreateMesh();

    }

    void CreateBlock(int BlockId, Vector3 BlockPos, bool[,,] ChunkData)
    {

        bool[] drawFace = CheckVoxel(ChunkData, BlockPos);

        for (int p = 0; p < 6; p++)
        {
            if (drawFace[p])
            {
                vertices.Add(BlockPos + voxelVerts[voxelTris[p, 0]]);
                vertices.Add(BlockPos + voxelVerts[voxelTris[p, 1]]);
                vertices.Add(BlockPos + voxelVerts[voxelTris[p, 2]]);
                vertices.Add(BlockPos + voxelVerts[voxelTris[p, 3]]);
                AddTexture(blockTexture[BlockId - 1].GetTexture(p));
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }

        }
    }

    void CreatChunk(bool[,,] ChunkData)
    {
        int blockId = 0;
        for(float x = 0; x < ChunkWidth; x++)
        {
            for (float z = 0; z < ChunkWidth; z++)
            {
                int blockHight = Mathf.FloorToInt(ChunkHeight - noise.perlin((x+xOffset) *scale +.01f, (z + zOffset) * scale +.01f, octaves, persistane) * (ChunkHeight/ ChunkReducer));
                for (float y = 0; y < blockHight; y++)
                {
                    if (y == 0)
                    {
                        blockId = 4;
                    }
                    else if (y == blockHight-1)
                    {
                        blockId = 1;
                    }
                    else if (y == blockHight-2)
                    {
                        blockId = 2;
                    }

                    else
                    {
                        blockId = 3;
                    }

                    foreach (ThreeDNoiseFeatures dNoiseFeature in threeDNoiseFeatures)
                    {
                        blockId = dNoiseFeature.ThreeDBlockToPlace(new Vector3(x + xOffset, y, z + zOffset), blockId);

                    }

                    if (blockId != 0)
                    {
                        CreateBlock(blockId, new Vector3(x, y, z), ChunkData);
                    }
                }
            }
        }
    }

    void BlockCheck()
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    ChunkData[x, y, z] = false;
                }
            }
        }
        for (float x = 0; x < ChunkWidth; x++)
        {
            for (float z = 0; z < ChunkWidth; z++)
            {
                for (float y = 0; y < Mathf.FloorToInt(ChunkHeight - noise.perlin((x + xOffset) * scale + .01f, (z + zOffset) * scale + .01f, octaves, persistane) * (ChunkHeight / ChunkReducer)); y++)
                {
                    ChunkData[Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z)] = true;
                    int i = 0;
                    foreach (ThreeDNoiseFeatures dNoiseFeature in threeDNoiseFeatures)
                    {
                        if(i>  0)
                        {
                            if(ChunkData[Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z)]==false)
                            {
                                continue;
                            }
                            
                        }
                        i++;
                        ChunkData[Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z)] = dNoiseFeature.ThreeDBlockToPlace(new Vector3(x + xOffset, y, z + zOffset), 1 ) != 0;
                    }
                }
            }
        }
    }

    bool[] CheckVoxel(bool[,,] ChunData, Vector3 pos)
    {
        bool[] DrawFace = new bool[6];

        Vector3 backFace = Vector3.back + pos;
        Vector3 frontFace = Vector3.forward + pos;
        Vector3 topFace = Vector3.up + pos;
        Vector3 bottomFace = Vector3.down + pos;
        Vector3 leftFace = Vector3.left + pos;
        Vector3 rightFace = Vector3.right + pos;

        try
        {
            DrawFace[0] = !ChunData[(int)backFace.x, (int)backFace.y, (int)backFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[0] = true;
        }
        try
        {
            DrawFace[1] = !ChunData[(int)frontFace.x, (int)frontFace.y, (int)frontFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[1] = true;
        }
        try
        {
            DrawFace[2] = !ChunData[(int)topFace.x, (int)topFace.y, (int)topFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[2] = true;
        }
        try
        {
            DrawFace[3] = !ChunData[(int)bottomFace.x, (int)bottomFace.y, (int)bottomFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[3] = true;
        }
        try
        {
            DrawFace[4] = !ChunData[(int)leftFace.x, (int)leftFace.y, (int)leftFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[4] = true;
        }
        try
        {
            DrawFace[5] = !ChunData[(int)rightFace.x, (int)rightFace.y, (int)rightFace.z];
        }
        catch (System.IndexOutOfRangeException)
        {
            DrawFace[5] = true;
        }




        return DrawFace;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / textureAtlasSize;
        float x = textureID - (y * textureAtlasSize);

        x *= NormalizedBlockTextureSize;
        y *= NormalizedBlockTextureSize;

        y = 1f - y - NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + NormalizedBlockTextureSize, y + NormalizedBlockTextureSize));


    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

}



