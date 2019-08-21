using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will be used to manage the chunks that composes the world, including loading/streaming
public class World : MonoBehaviour
{
    public static readonly int WORLD_WIDTH_IN_CHUNKS = 100;
    public static readonly int VIEW_DISTANCE_IN_CHUNKS = 5;

    public static int WORLD_WIDTH_IN_VOXELS
    {
        get { return WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH; }
    }

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    Chunk[,] chunks = new Chunk[WORLD_WIDTH_IN_CHUNKS, WORLD_WIDTH_IN_CHUNKS];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();

    ChunkCoord playerLastChunkCoord;
    ChunkCoord playerChunkCoord;

    public int seed;

    public BiomeAttributes biome;

    void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH) / 2f, Chunk.CHUNK_HEIGHT + 2f, (WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkFromVector3(spawnPosition);
        //CreateChunk(0, 0);
    }

    void Update()
    {
        playerChunkCoord = GetChunkFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoord = playerChunkCoord;
        }
    }

    void GenerateWorld()
    {
        for (int i = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; i < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; j < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                CreateChunk(i, j);
            }
        }
        player.position = spawnPosition;
    }

    void CreateChunk(int x, int y)
    {
        chunks[x, y] = new Chunk(this, new ChunkCoord(x, y));
        activeChunks.Add(new ChunkCoord(x, y));
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkFromVector3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int i = coord.x - VIEW_DISTANCE_IN_CHUNKS; i < coord.x + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = coord.y - VIEW_DISTANCE_IN_CHUNKS; j < coord.y + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                if (IsChunkInWorld(new ChunkCoord(i, j)))
                {
                    if (chunks[i, j] == null)
                    {
                        CreateChunk(i, j);
                    }
                    else if (!chunks[i, j].isActive)
                    {
                        chunks[i, j].isActive = true;
                        activeChunks.Add(new ChunkCoord(i, j));
                    }
                }
                for (int k = 0; k < previouslyActiveChunks.Count; k++)
                {
                    if (previouslyActiveChunks[k].Equals(new ChunkCoord(i, j)))
                    {
                        previouslyActiveChunks.RemoveAt(k);
                    }
                }
            }
        }

        foreach (ChunkCoord item in previouslyActiveChunks)
        {
            chunks[item.x, item.y].isActive = false;
        }

    }

    ChunkCoord GetChunkFromVector3(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.CHUNK_WIDTH);
        int y = Mathf.FloorToInt(worldPosition.z / Chunk.CHUNK_WIDTH);

        return new ChunkCoord(x, y);
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x > 0 && coord.x < WORLD_WIDTH_IN_CHUNKS - 1 && coord.y > 0 && coord.y < WORLD_WIDTH_IN_CHUNKS - 1;
    }

    bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < WORLD_WIDTH_IN_VOXELS && position.y < Chunk.CHUNK_HEIGHT && position.z >= 0 && position.z < WORLD_WIDTH_IN_VOXELS;
    }

    public byte GetVoxel(Vector3 position)
    {
        int y = Mathf.FloorToInt(position.y);

        // Immutable pass - things that will always be the case, like outside the world is air

        if (!IsVoxelInWorld(position))
            return 0;
        if (y == 0)
            return 1; // If it is the lowest layer, return bedrock

        // First terrain pass - first height variation
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DNoise(new Vector2(position.x, position.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (y == terrainHeight)
            voxelValue = 3;
        else if (y < terrainHeight && y > terrainHeight - 4)
            voxelValue = 5;
        else if (y > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        // Second terrain pass - 
        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (y > lode.minHeight && y < lode.maxHeight)
                    if (Noise.Get3DNoise(position, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        return voxelValue;
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    // In Minecraft, some blocks have different textures on faces, like the dirt and grass, wood, etc
    // using the Header keyword, these will show up in the world object under TextureValues. We can set them in the
    // 
    [Header("Texture Values")]
    public int backFaceTextureID;
    public int frontFaceTextureID;
    public int topFaceTextureID;
    public int bottomFaceTextureID;
    public int leftFaceTextureID;
    public int rightFaceTextureID;
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTextureID;
            case 1:
                return frontFaceTextureID;
            case 2:
                return topFaceTextureID;
            case 3:
                return bottomFaceTextureID;
            case 4:
                return leftFaceTextureID;
            case 5:
                return rightFaceTextureID;
            default:
                Debug.Log("Invalid faceIndex in the blocktype gettextureid function");
                return 0;
        }

    }
}
