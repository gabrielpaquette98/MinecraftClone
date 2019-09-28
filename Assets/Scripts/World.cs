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

    public BiomeAttributes biome; // Contains data on height, density and variation of terrain, including nodes

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();

    bool isCreatingChunks;

    /// <summary>
    /// Generates the starting chunks for the spawn point and initializes the spawn position
    /// </summary>
    void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH) / 2f, Chunk.CHUNK_HEIGHT - 50f, (WORLD_WIDTH_IN_CHUNKS * Chunk.CHUNK_WIDTH) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    /// <summary>
    /// Updates the chunks, creating missing chunks (by coroutine), enabling or disabling chunks out or in the view distance
    /// 
    /// note: Here we really should add pooling for the chunks, would be much more efficient.
    /// </summary>
    void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }
    }

    /// <summary>
    /// Function to generate the first few chunks, filling with initial chunks
    /// </summary>
    void GenerateWorld()
    {
        for (int i = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; i < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = (WORLD_WIDTH_IN_CHUNKS / 2) - VIEW_DISTANCE_IN_CHUNKS; j < (WORLD_WIDTH_IN_CHUNKS / 2) + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                chunks[i, j] = new Chunk(this, new ChunkCoord(i, j), true);
                activeChunks.Add(new ChunkCoord(i, j));
            }
        }
        player.position = spawnPosition;
    }

    /// <summary>
    /// Co-routine that creates the chunk and leaves mid-way in case it is too cpu-heavy
    /// 
    /// note: Too much stutters might indicate that adding more coroutines would be better
    /// </summary>
    /// <returns> IEnumerator for the coroutine </returns>
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].y].InitChunk();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    /// <summary>
    /// Checks what chunks must be removed or added and fills the "to-add" chunk list
    /// </summary>
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int i = coord.x - VIEW_DISTANCE_IN_CHUNKS; i < coord.x + VIEW_DISTANCE_IN_CHUNKS; i++)
        {
            for (int j = coord.y - VIEW_DISTANCE_IN_CHUNKS; j < coord.y + VIEW_DISTANCE_IN_CHUNKS; j++)
            {
                ChunkCoord currentCoord = new ChunkCoord(i, j);
                if (IsChunkInWorld(currentCoord))
                {
                    if (chunks[i, j] == null)
                    {
                        chunks[i, j] = new Chunk(this, currentCoord, false);
                        chunksToCreate.Add(currentCoord);
                    }
                    else if (!chunks[i, j].isActive)
                    {
                        chunks[i, j].isActive = true;
                    }
                    activeChunks.Add(currentCoord);
                }

                for (int k = 0; k < previouslyActiveChunks.Count; k++)
                {
                    if (previouslyActiveChunks[k].Equals(currentCoord))
                    {
                        previouslyActiveChunks.RemoveAt(k);
                    }
                }
            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.y].isActive = false;
        }

    }

    /// <summary>
    /// Function to get a chunkcoord from a world position
    /// </summary>
    /// <param name="worldPosition"> World position </param>
    /// <returns> A chunkcoord based on the x and y position </returns>
    ChunkCoord GetChunkCoordFromVector3(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.CHUNK_WIDTH);
        int y = Mathf.FloorToInt(worldPosition.z / Chunk.CHUNK_WIDTH);

        return new ChunkCoord(x, y);
    }

    /// <summary>
    /// Function that returns a ref to a chunk from a world position
    /// </summary>
    /// <param name="worldPosition"> World position </param>
    /// <returns> A ref to the chunk </returns>
    public Chunk GetChunkFromVector3(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.CHUNK_WIDTH);
        int y = Mathf.FloorToInt(worldPosition.z / Chunk.CHUNK_WIDTH);

        return chunks[x, y];
    }

    /// <summary>
    /// Function that checks if a chunkcoord is within the world space
    /// </summary>
    /// <param name="coord"> chunkcoord to check </param>
    /// <returns> Boolean value that tells if the chunk is in the world </returns>
    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x > 0 && coord.x < WORLD_WIDTH_IN_CHUNKS - 1 && coord.y > 0 && coord.y < WORLD_WIDTH_IN_CHUNKS - 1;
    }

    /// <summary>
    /// Function that checks if the voxel at a position is within the world
    /// </summary>
    /// <param name="voxelPosition"> position of the voxel to check </param>
    /// <returns> A boolean value that tells if the voxel if within the world </returns>
    bool IsVoxelInWorld(Vector3 voxelPosition)
    {
        return voxelPosition.x >= 0 && voxelPosition.x < WORLD_WIDTH_IN_VOXELS && voxelPosition.y < Chunk.CHUNK_HEIGHT && voxelPosition.z >= 0 && voxelPosition.z < WORLD_WIDTH_IN_VOXELS;
    }

    /// <summary>
    /// Function that determines what voxel should be at the said position.
    /// </summary>
    /// <param name="voxelPosition"> World position of a voxel </param>
    /// <returns> What the type of the voxel should be </returns>
    public byte GetVoxel(Vector3 voxelPosition)
    {
        int y = Mathf.FloorToInt(voxelPosition.y);

        // Immutable pass - things that will always be the case, like outside the world is air

        if (!IsVoxelInWorld(voxelPosition))
            return 0;
        if (y == 0)
            return 1; // If it is the lowest layer, return bedrock

        // First terrain pass - first height variation
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DNoise(new Vector2(voxelPosition.x, voxelPosition.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
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
                    if (Noise.Get3DNoise(voxelPosition, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        return voxelValue;
    }

    /// <summary>
    /// Function that checks if the voxel at that position is set to a solid or not solid state.
    /// Determines if the voxel is to be visible or not
    /// </summary>
    /// <param name="voxelPosition"> A world position for the voxel checked </param>
    /// <returns> A boolean value that represents if the voxel is to be visible by the player or not </returns>
    public bool CheckForVoxel(Vector3 voxelPosition)
    {
        ChunkCoord thisChunk = new ChunkCoord(voxelPosition);

        if (!IsChunkInWorld(thisChunk) || voxelPosition.y < 0 || voxelPosition.y > Chunk.CHUNK_HEIGHT)
            return false;

        if (chunks[thisChunk.x, thisChunk.y] != null && chunks[thisChunk.x, thisChunk.y].isVoxelMapPopulated)
            return blocktypes[chunks[thisChunk.x, thisChunk.y].GetVoxelFromGlobalVector3(voxelPosition)].isSolid;

        return blocktypes[GetVoxel(voxelPosition)].isSolid;
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
