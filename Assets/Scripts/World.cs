using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will be used to manage the chunks that composes the world, including loading/streaming
public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blocktypes;
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
