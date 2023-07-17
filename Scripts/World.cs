using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class World : MonoBehaviour
{
    // material is a block atlas texture applied to a material
    public Material material;
    // assigning each part of block atlas to a block
    public BlockTexture[] blockTexture;
    // used for generating caves
    public ThreeDNoiseFeatures[] threeDNoiseFeatures;
    // the location of the player so that the world moves with the player and it seems that the world is infinite
    public Transform player;
    // Chunk Height and Width determine the size of each chunk
    public int ChunkHeight; // 128
    public int ChunkWidth; // 16

    // Seed is the variable that makes the world random, the same seed would mean the same world
    public int Seed;

    // how offset the entire world will be, can be used to make every world random
    Vector2 offsets;

    // assigning noise features
    public float scale; // 0.015
    public int octaves; // 4
    public float persistane; // 0.5
    public float chunkReducer; // .95

    // the amount of chunks rendered in each direction of the player
    public int RenderDist;

    // list of all the chunk positions that are currently active
    List<int[]> activeChunksMatrix;
    // the actual gameobject data of all the chunks that have ever been active
    List<GameObject> chunksofAllTime;
    // quadrants are used for the generation of the chunks
    int[][] quadrants;

    // these two vars are used to locate what chunk the player begins in
    int playerCenterX;
    int playerCenterZ;

    int[] blockIds;

    // an inbuilt unity function, called at the start of game, usually to instantiate variables
    void Start()
    {
        // initializing a lot of vars
        blockIds = new int[ChunkHeight];
        quadrants = new int[][] { new int[] { 1, 1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { -1, -1 } };
        playerCenterX = Mathf.FloorToInt(player.transform.position.x / ChunkWidth);
        playerCenterZ = Mathf.FloorToInt(player.transform.position.z / ChunkWidth);
        activeChunksMatrix = new List<int[]>();
        chunksofAllTime = new List<GameObject>();

        Seed = OptionsMenu.seed;
        RenderDist = OptionsMenu.renderDist;

        if (OptionsMenu.useRandomSeed)
        {
            Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        var randomGenerator = new System.Random(Seed);
        offsets = new Vector2(randomGenerator.Next(int.MinValue/75, int.MaxValue / 75), 0);

        populateBlockIds();

        CreateWorld(RenderDist + 2);
    }

    /*
     * xpos is the xpositon of the chunk
     * zpos is the zposition of the chunk
     * this function creates the Chunk GameObject
     * it names the chunk
     * adds all the required arguments to the chunk
     * and positions it in the world
     * this function is called for everytime a new chunk is created
     * it returns the chunk Game Object which is stored in the chunksofAllTime variable
    */
    GameObject ChunkCreate(int xpos, int zpos)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Chunk [{xpos}, {zpos}]";
        gameObject.transform.SetParent(transform);
        gameObject.transform.position = new Vector3(xpos * ChunkWidth, 0, zpos * ChunkWidth);
        gameObject.layer = 3;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        Chunk chunk = gameObject.AddComponent<Chunk>();

        chunk.xPos = xpos;
        chunk.zPos = zpos;

        meshRenderer.material = material;
        chunk.blockTexture = blockTexture;

        chunk.meshFilter = meshFilter;
        chunk.meshRenderer = meshRenderer;
        chunk.meshCollider = meshCollider;

        chunk.ChunkHeight = ChunkHeight;
        chunk.ChunkWidth = ChunkWidth;

        chunk.scale = scale;
        chunk.octaves = octaves;
        chunk.persistane = persistane;

        chunk.ChunkReducer = chunkReducer;
        chunk.xOffset = xpos * ChunkWidth + offsets.x;
        chunk.zOffset = zpos * ChunkWidth + offsets.y;

        chunk.threeDNoiseFeatures = threeDNoiseFeatures;

        chunk.blockIds = blockIds;

        return gameObject;
    }

    void populateBlockIds()
    {
        for (int y = 0; y < ChunkHeight; y++)
        {
            if (y <= 20)
            {
                blockIds[y] = 5;
            }
            else if (y <= 55)
            {
                blockIds[y] = 4;
            }
            else
            {
                blockIds[y] = 3;
            }
        }
    }

    /*
     * Uses _RenderDist to know how many chunks in each direction to generate, i.e. a _RenderDist of 2 would mean a 4X4 of chunks
     * First it iterates through the quadrants array to know how to generate the chunks around the center, usually the player location
     * As it iterates through every place a chunk should exist it checks if the chunk has already generated and is just in in systme memory in the chunksofAllTime array, if it isn't then it generates a new chunk, but if it is then it just activates the old chunk
    */
    void CreateWorld(int _RenderDist)
    {
        foreach (int[] i in quadrants)
        {
            for (int x = 0; x < _RenderDist; x++)
            {
                for (int z = 0; z < _RenderDist; z++)
                {
                    int[] chunkCoords = new int[] { x * i[0] + playerCenterX, z * i[1] + playerCenterZ };
                    if (!CompareIntList(activeChunksMatrix, chunkCoords))
                    {
                        int chunkIndex = findChunkFromCoords(chunksofAllTime, chunkCoords);
                        if (chunkIndex == -1)
                        {
                            GameObject chunk = ChunkCreate(x * i[0] + playerCenterX, z * i[1] + playerCenterZ);
                            activeChunksMatrix.Add(chunkCoords);
                            chunksofAllTime.Add(chunk);
                        }
                        else
                        {
                            GameObject chunk = chunksofAllTime[chunkIndex];
                            chunk.SetActive(true);
                            activeChunksMatrix.Add(chunkCoords);
                        }

                    }
                }
            }
        }
        
    }

    // an inbuilt unity framework, called every frame
    void Update()
    {
        RenderDist = OptionsMenu.renderDist;
        UpdateChunks();
    }

    /*
     * As the player moves the moves through the world the world center updates
     * Whenever the world center changes all the chunks are disabled, these chunks are are stored in system memory
     * Then the create world function is called so that the chunks are created according to the new center
    */
    void UpdateChunks()
    {
        if (playerCenterX != Mathf.FloorToInt(player.transform.position.x / ChunkWidth))
        {
            playerCenterX = Mathf.FloorToInt(player.transform.position.x / ChunkWidth);
            DisableAllActiveChunks();
            CreateWorld(RenderDist);
        }
        if (playerCenterZ != Mathf.FloorToInt(player.transform.position.z / ChunkWidth))
        {
            playerCenterZ = Mathf.FloorToInt(player.transform.position.z / ChunkWidth);
            DisableAllActiveChunks();
            CreateWorld(RenderDist);
        }
    }

    /*
     * Finds index of int[] in List<[]>
     * This function is used and not just a simple indexOf() function, because when comparing objects if the two objects were created at different instances the objects would be totaly different
     * This overcomes that by checking if both values inside the int[] match
    */

    bool CompareIntList(List<int[]> compareList, int[] compareVal)
    {
        bool returnVal = false;

        foreach (int[] compare in compareList)
        {
            returnVal = compare[0] == compareVal[0] && compare[1] == compareVal[1];
            if (returnVal) break;
        }

        return returnVal;
    }

    // Gets Chunk Object by knowing the coords of that chunk
    int findChunkFromCoords(List<GameObject> compareList, int[] compareVal)
    {
        for(int i = 0; i < compareList.Count; i++)
        {
            if (compareList[i].GetComponent<Chunk>().xPos == compareVal[0] && compareList[i].GetComponent<Chunk>().zPos == compareVal[1]) return i;
        }

        return -1;
    }

    // Disables all the chunks currently active in the game
    void DisableAllActiveChunks()
    {
        foreach (int[] chunkCoords in activeChunksMatrix)
        {
            GameObject.Find($"Chunk [{chunkCoords[0]}, {chunkCoords[1]}]").SetActive(false);

        }
        activeChunksMatrix = new List<int[]>();
    }
}

// This class assigns the location of textures for each block in the texture atlas 
[Serializable]
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

    // function returns texture location for block when provided what side of block the texture is needed for
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

// This class is usefull in generating caves and other 3D noise features
[Serializable]
public class ThreeDNoiseFeatures
{
    public string featureName;

    // The noise scale
    public float ThreeDScale;
    // minimum value has to be returned by noise function for feature to generate
    public float ThreeDThreshold;

    // what block will be generated; air is 0
    public int BlockID;

    // the y-values where the feature can generate
    public int GenerationMaxHeight;
    public int GenerationMinHeight;

    // how many layers of noise will be combined
    public int octaves;
    // how much each octave layer impacts the amplitude
    public float persistence;

    // how much to offset the features by, usefull for random world gen
    public Vector3 offset;

    Noise noise = new Noise();

    // Used to verify where to place feature
    public int ThreeDBlockToPlace(Vector3 pos, int currentBlockId)
    {
        float num = noise.ThreeDNoise(pos.x + offset.x, pos.y + offset.y, pos.z + offset.z, ThreeDScale, octaves, persistence);
        if (num > ThreeDThreshold && pos.y > GenerationMinHeight + noise.perlin((pos.x + offset.x) * ThreeDScale*2, (pos.z + offset.z) * ThreeDScale*2, octaves, persistence) * 8 && pos.y < GenerationMaxHeight - noise.perlin((pos.x + offset.x)* ThreeDScale*2, (pos.z + offset.z)* ThreeDScale*2, octaves, persistence)*8 )
            return BlockID;
        return currentBlockId;
    }

}


/*
 Credits:
    [1] The Sky Box Is from: https://assetstore.unity.com/packages/2d/textures-materials/sky/galaxybox-1-0-18289
    [2] The Game Engine Used Is Unity Game Engine, it allow me to create and compile my game: https://unity.com/
*/