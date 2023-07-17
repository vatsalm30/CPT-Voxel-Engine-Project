using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    // Voxel Verts defines all of the verticies of each voxel
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

    // Defines how each side is creates, and to make it more preformant I have made each list smaller as it was redundent
    int[,] voxelTris = new int[6, 4]
    {
        {0, 3, 1, 2}, // Back face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6} // Right
    };

    // The Renderer of the mesh
    public MeshRenderer meshRenderer;
    // The mesh
    public MeshFilter meshFilter;
    // The collider
    public MeshCollider meshCollider;
    // The textures of each block; derived from texture atlas
    public BlockTexture[] blockTexture;

    // Chunk Height and Width
    public int ChunkHeight;
    public int ChunkWidth;

    // 3D int array that stores what type of block exists
    public int[,,] ChunkData;
    // Lookup table for blockIds
    public int[] blockIds;

    // Noise Features
    public float scale;
    public int octaves;
    public float persistane;
    public float ChunkReducer;

    // Stores all the vertices of each block in the chunck
    List<Vector3> vertices = new List<Vector3>();
    // Stores all trangle meshes in the world
    List<int> triangles = new List<int>();
    // Stores where textures will be placed
    List<Vector2> uvs = new List<Vector2>();

    // How large texture atlas is; 4X4
    int textureAtlasSize = 4;

    // How many verticies there are in the chunk
    int vertexIndex = 0;
    // How much the chunk is offset on x and y
    public float xOffset;
    public float zOffset;

    // The location of the chunk in chunk matrix
    public int xPos;
    public int zPos;

    // 3D noise features, like caves and such
    public ThreeDNoiseFeatures[] threeDNoiseFeatures;

    float NormalizedBlockTextureSize
    {
        get { return 1f / (float)textureAtlasSize; }
    }


    Noise noise;

    // an inbuilt unity function, called at the start of game, usually to instantiate variables
    void Start()
    {
        noise = new Noise();
        ChunkData = new int[ChunkWidth,ChunkHeight, ChunkWidth];

        BlockCheck();

        CreateChunk(ChunkData);
        
        CreateMesh();
    }


    /*
     * Creates each block in chunk and checks if each face needs to be drawn; no point in drawing block face that is blocked by another block
     * BlockPos is where the block is in the chunk
     * ChunkData is all of the blocks in the game
    */
    void CreateBlock(Vector3 BlockPos, int[,,] ChunkData)
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
                AddTexture(blockTexture[ChunkData[(int)BlockPos.x, (int)BlockPos.y, (int)BlockPos.z] - 1].GetTexture(p));
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

    /*
     * This function creates the terrain by looping through all the (x, y, z) coords of the chunk 
     * It checks if the block is an airblock (0) or not, and if it isn't it creates a block there
    */
    void CreateChunk(int[,,] ChunkData)
    {
        for(int x = 0; x < ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkWidth; z++)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    if(ChunkData[x, y, z] != 0)CreateBlock(new Vector3(x, y, z), ChunkData);

                }
            }
        }
    }

    /*
     * This function is really important as it fills both the ChunkData and BlockHeights arrays
     * These arrays are used to make the world efficiently and to make it preform efficiently
     * All the block heights are iterated through 
     * And 3D noise features are produced
     */
    void BlockCheck()
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkWidth; z++)
            {
                int blockHeight = Mathf.FloorToInt(ChunkHeight - noise.perlin((x + xOffset) * scale + .01f, (z + zOffset) * scale + .01f, octaves, persistane) * (ChunkHeight / ChunkReducer));
                for (int y = 0; y < blockHeight; y++)
                {
                    int blockId = blockIds[y];

                    if (y == blockHeight - 1)
                    {
                       blockId = 1;
                    }
                    else if (y >= blockHeight - 5)
                    {
                        blockId = 2;
                    }
                    ChunkData[x, y, z] = blockId;
                    foreach (ThreeDNoiseFeatures dNoiseFeature in threeDNoiseFeatures)
                    {
                        ChunkData[x, y, z] = dNoiseFeature.ThreeDBlockToPlace(new Vector3(x + xOffset, y, z + zOffset), ChunkData[x, y, z]);
                    }
                }
            }
        }
    }

    /*
     * Takes chunk data and the position of the block
     * Adds true to the bool[] if there is no block next to it otherwise it adds false
     * If the block is outside the world and on the bottom it adds a false to that face, otherwise true; 
     * Wherever it is outside of the bounds of the array there should be a check to see if that face should be created, but there isn't because I found that it is simpler and faster to just put a predetermined true or false
     */
    public bool[] CheckVoxel(int[,,] ChunkData, Vector3 pos)
    {
        bool[] DrawFace = new bool[6];

        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        if (x > 0)
        {
            DrawFace[4] = ChunkData[x - 1, y, z] == 0;
        }
        else
        {
            DrawFace[4] = true;
        }

        if (x < ChunkWidth - 1)
        {
            DrawFace[5] = ChunkData[x + 1, y, z] == 0;
        }
        else
        {
            DrawFace[5] = true;
        }

        if (y < ChunkHeight - 1)
        {
            DrawFace[2] = ChunkData[x, y + 1, z] == 0;
        }
        else
        {
            DrawFace[2] = true;
        }

        if (y > 0)
        {
            DrawFace[3] = ChunkData[x, y - 1, z] == 0;
        }
        else
        {
            DrawFace[3] = false;
        }

        if (z > 0)
        {
            DrawFace[0] = ChunkData[x, y, z - 1] == 0;
        }
        else
        {
            DrawFace[0] = true;
        }

        if (z < ChunkWidth - 1)
        {
            DrawFace[1] = ChunkData[x, y, z + 1] == 0;
        }
        else
        {
            DrawFace[1] = true;
        }

        return DrawFace;
    }

    // Adds textures by using fractions of the actual texture atlas
    // This is done because it is easier to load one big texture file than alot of large texture file
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

    // Uses the verticies, triangles, and uv arrays to initilize a mesh
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

}



