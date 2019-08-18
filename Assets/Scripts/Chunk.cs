//------------------------------------Step 1: The first voxel, with texture/uvs--------------------------------

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//// Data needed to create and manage a chunk.
//public class Chunk : MonoBehaviour
//{
//    // Used to create and display a mesh, which is a visual representation of the squares we will display. 
//    public MeshRenderer meshRenderer;   // Displays the mesh, and manages how it is displayed (material, etc)
//    public MeshFilter meshFilter;       // Stores mesh data
//    // note: for the purpose of testing, we can make an empty gameobject, name it Chunk, and add it
//    //       both the attributes above. Add our script and link them to the attributes. Could it be
//    //       left "public", or should we change it to "[SerializeField]" ?

//    // Used to stode the vertices, triangles and texture/material data of the chunk
//    int vertexIndex = 0;
//    List<Vector3> meshVertices = new List<Vector3>();
//    List<int> meshTriangles = new List<int>();
//    List<Vector2> uvs = new List<Vector2>();

//    void Start()
//    {
//        AddVoxelDataToChunk();
//        CreateChunkMesh();
//    }

//    void AddVoxelDataToChunk()
//    {
//        for (int i = 0; i < 6; i++)
//        {
//            for (int j = 0; j < 6; j++)
//            {
//                int triangleIndex = VoxelData.voxelTriangles[i, j];
//                meshVertices.Add(VoxelData.voxelVertices[triangleIndex]);
//                meshTriangles.Add(vertexIndex);
//                uvs.Add(VoxelData.voxelUvs[j]);

//                vertexIndex++;
//            }
//        }
//    }

//    void CreateChunkMesh()
//    {
//        Mesh mesh = new Mesh();
//        mesh.vertices = meshVertices.ToArray();
//        mesh.triangles = meshTriangles.ToArray();
//        mesh.uv = uvs.ToArray(); // for use with the material and texture. We make a materials folder and add a material named Voxels
//        // the material will not be smooth nor metallic. In chunk's mesh renderer drag and drop the new material
//        // We can make a new textures folder and add to it the given texture file.
//        // We will pull the ArrowTexture in the folder, put the max size to 32, wrap mode to Clamp and filter to point for no texture smoothing
//        // Dont forget to apply the changes to the texture. Change the voxel material to use the texture by clicking on the albedo dot.

//        mesh.RecalculateNormals();

//        meshFilter.mesh = mesh;
//    }
//}
//---------------------------------End of Step 1: The first voxel, with texture/uvs----------------------------

////------------------------------------------Step 2: The first chunk--------------------------------------------
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//// Data needed to create and manage a chunk.
//public class Chunk : MonoBehaviour
//{
//    public static readonly int CHUNK_WIDTH = 5;
//    public static readonly int CHUNK_HEIGHT = 16;

//    // Used to create and display a mesh, which is a visual representation of the squares we will display. 
//    public MeshRenderer meshRenderer;   // Displays the mesh, and manages how it is displayed (material, etc)
//    public MeshFilter meshFilter;       // Stores mesh data
//    // note: for the purpose of testing, we can make an empty gameobject, name it Chunk, and add it
//    //       both the attributes above. Add our script and link them to the attributes. Could it be
//    //       left "public", or should we change it to "[SerializeField]" ?

//    // Used to stode the vertices, triangles and texture/material data of the chunk
//    int vertexIndex = 0;
//    List<Vector3> meshVertices = new List<Vector3>();
//    List<int> meshTriangles = new List<int>();
//    List<Vector2> uvs = new List<Vector2>();

//    // array that keeps track of which cube are to be included in the mesh and which are invisible
//    bool[,,] voxelMap = new bool[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];
//    void Start()
//    {
//        PopulateVoxelMap();
//        CreateChunkMeshData();
//        CreateChunkMesh();
//    }

//    void CreateChunkMeshData()
//    {
//        for (int x = 0; x < CHUNK_WIDTH; x++)
//        {
//            for (int y = 0; y < CHUNK_HEIGHT; y++)
//            {
//                for (int z = 0; z < CHUNK_WIDTH; z++)
//                {
//                    AddVoxelDataToChunk(new Vector3(x, y, z));
//                }
//            }
//        }
//    }

//    bool CheckVoxel(Vector3 voxelPosition)
//    {
//        int x = Mathf.FloorToInt(voxelPosition.x);
//        int y = Mathf.FloorToInt(voxelPosition.y);
//        int z = Mathf.FloorToInt(voxelPosition.z);

//        if (x < 0 || x > CHUNK_WIDTH - 1 || y < 0 || y > CHUNK_HEIGHT - 1 || z < 0 || z > CHUNK_WIDTH - 1)
//            return false; // if any of these are true, then the voxel is outside of the chunk

//        return voxelMap[x, y, z];
//    }

//    void PopulateVoxelMap()
//    {
//        for (int x = 0; x < CHUNK_WIDTH; x++)
//        {
//            for (int y = 0; y < CHUNK_HEIGHT; y++)
//            {
//                for (int z = 0; z < CHUNK_WIDTH; z++)
//                {
//                    voxelMap[x, y, z] = true;
//                }
//            }
//        }
//    }

//    void AddVoxelDataToChunk(Vector3 chunkPosition)
//    {
//        for (int i = 0; i < 6; i++)
//        {
//            if (!CheckVoxel(chunkPosition + VoxelData.voxelFaceChecks[i]))
//            {
//                for (int j = 0; j < 4; j++)
//                {
//                    meshVertices.Add(chunkPosition + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);
//                    uvs.Add(VoxelData.voxelUvs[j]);
//                }
//                meshTriangles.Add(vertexIndex);
//                meshTriangles.Add(vertexIndex + 1);
//                meshTriangles.Add(vertexIndex + 2);
//                meshTriangles.Add(vertexIndex + 2);
//                meshTriangles.Add(vertexIndex + 1);
//                meshTriangles.Add(vertexIndex + 3);
//                vertexIndex += 4;
//            }
//        }
//    }

//    void CreateChunkMesh()
//    {
//        Mesh mesh = new Mesh();
//        mesh.vertices = meshVertices.ToArray();
//        mesh.triangles = meshTriangles.ToArray();
//        mesh.uv = uvs.ToArray(); // for use with the material and texture. We make a materials folder and add a material named Voxels
//        // the material will not be smooth nor metallic. In chunk's mesh renderer drag and drop the new material
//        // We can make a new textures folder and add to it the given texture file.
//        // We will pull the ArrowTexture in the folder, put the max size to 32, wrap mode to Clamp and filter to point for no texture smoothing
//        // Dont forget to apply the changes to the texture. Change the voxel material to use the texture by clicking on the albedo dot.

//        mesh.RecalculateNormals();

//        meshFilter.mesh = mesh;
//    }
//}
////---------------------------------------End of Step 2: The first chunk----------------------------------------
///
//------------------------------------------Step 3: World object and texturing--------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Data needed to create and manage a chunk.
public class Chunk : MonoBehaviour
{
    public static readonly int CHUNK_WIDTH = 5;
    public static readonly int CHUNK_HEIGHT = 16;

    // Used to create and display a mesh, which is a visual representation of the squares we will display. 
    public MeshRenderer meshRenderer;   // Displays the mesh, and manages how it is displayed (material, etc)
    public MeshFilter meshFilter;       // Stores mesh data
    // note: for the purpose of testing, we can make an empty gameobject, name it Chunk, and add it
    //       both the attributes above. Add our script and link them to the attributes. Could it be
    //       left "public", or should we change it to "[SerializeField]" ?

    // Used to stode the vertices, triangles and texture/material data of the chunk
    int vertexIndex = 0;
    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    // array that keeps track of which cube are to be included in the mesh and which are invisible
    // bool[,,] voxelMap = new bool[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    // array to indicate what type of block is used. Air is not added to the mesh data
    byte[,,] voxelMap = new byte[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];

    World world;
    
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        PopulateVoxelMap();
        CreateChunkMeshData();
        CreateChunkMesh();
    }

    /// <summary>
    /// Function that creates the mesh data and add all the voxels to the chunk
    /// </summary>
    void CreateChunkMeshData()
    {
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                for (int z = 0; z < CHUNK_WIDTH; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
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

        if (x < 0 || x > CHUNK_WIDTH - 1 || y < 0 || y > CHUNK_HEIGHT - 1 || z < 0 || z > CHUNK_WIDTH - 1)
            return false; // if any of these are true, then the voxel is outside of the chunk

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
                    if (y < 1)
                        voxelMap[x, y, z] = 0;
                    else if (y == CHUNK_HEIGHT - 1)
                        voxelMap[x, y, z] = 2;
                    else
                        voxelMap[x, y, z] = 1;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new voxel and adds it to the mesh data of the chunk
    /// </summary>
    /// <param name="chunkPosition">World position of the chunk</param>
    void AddVoxelDataToChunk(Vector3 chunkPosition)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(chunkPosition + VoxelData.voxelFaceChecks[i]))
            {
                for (int j = 0; j < 4; j++)
                {
                    meshVertices.Add(chunkPosition + VoxelData.voxelVertices[VoxelData.voxelTriangles[i, j]]);
                }
                byte blockID = voxelMap[(int)chunkPosition.x, (int)chunkPosition.y, (int)chunkPosition.z];
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

        uvs.Add(new Vector2(x,y));
        uvs.Add(new Vector2(x,y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}
//---------------------------------------End of Step 3: World object and texturing----------------------------------------