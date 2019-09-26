using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data needed to create and manage a chunk.
public class Chunk
{
    public static readonly int CHUNK_WIDTH = 16;
    public static readonly int CHUNK_HEIGHT = 128;

    // Used to create and display a mesh, which is a visual representation of the squares we will display. 
    MeshRenderer meshRenderer;   // Displays the mesh, and manages how it is displayed (material, etc)
    MeshFilter meshFilter;       // Stores mesh data
    // note: for the purpose of testing, we can make an empty gameobject, name it Chunk, and add it
    //       both the attributes above. Add our script and link them to the attributes. Could it be
    //       left "public", or should we change it to "[SerializeField]" ?

    // Used to store the vertices, triangles and texture/material data of the chunk
    int vertexIndex = 0;
    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    // array to indicate what type of block is used. Air is not added to the mesh data
    public byte[,,] voxelMap = new byte[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    World world;
    GameObject chunkGameObject;

    public ChunkCoord coord;

    private bool isChunkActive;

    public bool isVoxelMapPopulated = false;

    public Chunk(World worldReference, ChunkCoord position, bool generateOnLoad)
    {
        world = worldReference;
        coord = position;
        isActive = true;

        if (generateOnLoad)
        {
            InitChunk();
        }
    }

    public void InitChunk()
    {
        chunkGameObject = new GameObject();
        meshFilter = chunkGameObject.AddComponent<MeshFilter>();
        meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkGameObject.transform.SetParent(world.transform);
        chunkGameObject.transform.position = new Vector3(coord.x * CHUNK_WIDTH, 0, coord.y * CHUNK_WIDTH);
        chunkGameObject.name = "Chunk " + coord.x + ", " + coord.y;

        PopulateVoxelMap();
        UpdateChunkData();
    }

    /// <summary>
    /// Function that creates the mesh data and add all the voxels to the chunk
    /// </summary>
    void UpdateChunkData()
    {
        ClearMeshData();

        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                for (int z = 0; z < CHUNK_WIDTH; z++)
                {
                    if (world.blocktypes[voxelMap[x, y, z]].isSolid)
                    {
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                    }
                }
            }
        }

        CreateChunkMesh();
    }

    /// <summary>
    /// Function to check if the voxel should be added to mesh data to be displayed or not
    /// </summary>
    /// <param name="voxelPosition">Position of the voxel relative to the chunk</param>
    /// <returns></returns>
    bool CheckVoxel(Vector3 voxelPosition)
    {
        int x = Mathf.FloorToInt(voxelPosition.x);
        int y = Mathf.FloorToInt(voxelPosition.y);
        int z = Mathf.FloorToInt(voxelPosition.z);

        if (!IsVoxelInChunk(x, y, z))
            return world.CheckForVoxel(voxelPosition + position);

        return world.blocktypes[voxelMap[x, y, z]].isSolid; // if it is solid, then it will be displayed
    }

    /// <summary>
    /// Function that chooses and initialises the voxelmap, which contains the blocktype id of all blocks
    /// </summary>
    void PopulateVoxelMap()
    {
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                for (int z = 0; z < CHUNK_WIDTH; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }

        isVoxelMapPopulated = true;
    }

    /// <summary>
    /// Creates a new voxel and adds it to the mesh data of the chunk
    /// </summary>
    /// <param name="voxelPosition">World position of the chunk</param>
    void AddVoxelDataToChunk(Vector3 voxelPosition)
    {
        for (int i = 0; i < 6; i++) // for each faces of the cube/voxel
        {
            if (!CheckVoxel(voxelPosition + VoxelData.voxelFaceChecks[i])) // if the voxel adjacent to the face is not solid (meaning this face is visible)
            {
                for (int j = 0; j < 4; j++) // Add all four vertices of the face
                {
                    meshVertices.Add(voxelPosition + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);
                }
                byte blockID = voxelMap[(int)voxelPosition.x, (int)voxelPosition.y, (int)voxelPosition.z];
                AddTexture(world.blocktypes[blockID].GetTextureID(i));

                meshTriangles.Add(vertexIndex);
                meshTriangles.Add(vertexIndex + 1);
                meshTriangles.Add(vertexIndex + 2);
                meshTriangles.Add(vertexIndex + 2);
                meshTriangles.Add(vertexIndex + 1);
                meshTriangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    public byte GetVoxelFromGlobalVector3(Vector3 globalPosition)
    {
        int x = Mathf.FloorToInt(globalPosition.x);
        int y = Mathf.FloorToInt(globalPosition.y);
        int z = Mathf.FloorToInt(globalPosition.z);

        x -= Mathf.FloorToInt(position.x);
        z -= Mathf.FloorToInt(position.z);

        return voxelMap[x, y, z];
    }

    /// <summary>
    /// Creates the base mesh data from the lists
    /// </summary>
    void CreateChunkMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.uv = uvs.ToArray(); // for use with the material and texture. We make a materials folder and add a material named Voxels
        // the material will not be smooth nor metallic. In chunk's mesh renderer drag and drop the new material
        // We can make a new textures folder and add to it the given texture file.
        // We will pull the ArrowTexture in the folder, put the max size to 32, wrap mode to Clamp and filter to point for no texture smoothing
        // Dont forget to apply the changes to the texture. Change the voxel material to use the texture by clicking on the albedo dot.

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        meshTriangles.Clear();
        meshVertices.Clear();
        uvs.Clear();
    }

    public void EditVoxel(Vector3 voxelPosition, byte newBlockID)
    {
        int x = Mathf.FloorToInt(voxelPosition.x);
        int y = Mathf.FloorToInt(voxelPosition.y);
        int z = Mathf.FloorToInt(voxelPosition.z);

        x -= Mathf.FloorToInt(position.x);
        z -= Mathf.FloorToInt(position.z);

        voxelMap[x, y, z] = newBlockID;

        UpdateSurroundingVoxels(x, y, z);

        UpdateChunkData();
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.voxelFaceChecks[i];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                // Update other chunks if the adjacent voxel is not in this chunk
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunkData();
            }
        }
    }

    /// <summary>
    /// Function to add a texture to the chunk mesh. Needs a new material configured as clamp with no filter, using an unlit texture.
    /// Do not forget to put the material in the world script parameter
    /// The World object choses what blocks
    /// And we must also drag and drop the material to our test chunk object
    /// </summary>
    /// <param name="textureID">The number of the texture in the texture atlas</param>
    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || x > CHUNK_WIDTH - 1 || y < 0 || y > CHUNK_HEIGHT - 1 || z < 0 || z > CHUNK_WIDTH - 1);
    }

    public bool isActive
    {
        get { return isChunkActive; }
        set
        {
            isChunkActive = value;
            if (chunkGameObject != null)
            {
                chunkGameObject.SetActive(value);
            }
        }
    }

    public Vector3 position
    {
        get { return chunkGameObject.transform.position; }
    }
}

public class ChunkCoord
{
    public int x;
    public int y;

    public ChunkCoord(Vector3 position)
    {
        int _x = Mathf.FloorToInt(position.x);
        int _z = Mathf.FloorToInt(position.z);

        x = _x / Chunk.CHUNK_WIDTH;
        y = _z / Chunk.CHUNK_WIDTH;
    }

    public ChunkCoord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        return other.x == x && other.y == y;
    }
}