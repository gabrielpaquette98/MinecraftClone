using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /// <summary>
    /// Generates a random 2D Perlin noise value
    /// </summary>
    /// <param name="position">Where on the noise texture to pick the value</param>
    /// <param name="offset">Diagonal offset to the position to pick the value</param>
    /// <param name="scale">Zoom on the texture, or frequency of the noise</param>
    /// <returns></returns>
    public static float Get2DNoise(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.01f) / Chunk.CHUNK_WIDTH * scale + offset, (position.y + 0.01f) / Chunk.CHUNK_WIDTH * scale + offset);
    }

    public static bool Get3DNoise(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + 0.01f) * scale;
        float y = (position.y + offset + 0.01f) * scale;
        float z = (position.z + offset + 0.01f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return (AB + BC + AC + BA + CB + CA) / 6 > threshold;
    }
}
