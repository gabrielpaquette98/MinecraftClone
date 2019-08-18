using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// "VoxelData" stores needed data to represent a voxel in a shape of a cube.
// It will mostly contain look-up tables about the voxels
public static class VoxelData
{
    // Texture data. Texture coordinates between 0 and 1 to choose what part of the texture to display
    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    // Array of relative vertices coordinates. 
    public static readonly Vector3[] voxelVertices = new Vector3[8]
    {   // The vertices are ordered as (x, y, z) with y being the height
        new Vector3(0.0f, 0.0f, 0.0f), // 1
        new Vector3(1.0f, 0.0f, 0.0f), // 2
        new Vector3(1.0f, 1.0f, 0.0f), // 3
        new Vector3(0.0f, 1.0f, 0.0f), // 4
        new Vector3(0.0f, 0.0f, 1.0f), // 5
        new Vector3(1.0f, 0.0f, 1.0f), // 6
        new Vector3(1.0f, 1.0f, 1.0f), // 7 
        new Vector3(0.0f, 1.0f, 1.0f)  // 8
    };

    // Triangles forming the cube
    public static readonly int[,] voxelTriangles = new int[6, 4]
    {
        // Represents the 6 faces of the cube. Two triangles per face, three vertices per triangle.
        { 0, 3, 1, 2 }, // Back, triangle 1 is made of vertices 0, 3, and 1.
        { 5, 6, 4, 7 }, // Front
        { 3, 7, 2, 6 }, // Top
        { 1, 5, 0, 4 }, // Bottom
        { 4, 7, 0, 3 }, // Left
        { 1, 2, 5, 6 }  // Right
    };

    // texture mapping data. Used to display certain texture coordinates on which cube face we want.
    public static readonly Vector2[] voxelUvs = new Vector2[4] // 6 because there are 6 vertices referenced in a face, 3 per triangle
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };

    //Array of positions describing adjacents voxels meant to know what face of the cube to display
    // to use them the voxels added must be kept in memory
    public static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0.0f,  0.0f, -1.0f),
        new Vector3(0.0f,  0.0f,  1.0f),
        new Vector3(0.0f,  1.0f,  0.0f),
        new Vector3(0.0f, -1.0f,  0.0f),
        new Vector3(-1.0f, 0.0f,  0.0f),
        new Vector3(1.0f,  0.0f,  0.0f)
    };

}