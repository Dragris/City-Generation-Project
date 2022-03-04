using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Making sure it's not 0 so the division does not crash, even if this is controlled with the range
        if (scale <=  0) {
            scale = 0.0001f;
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
                noiseMap [x, y] = perlinValue;
            }
        }

        return noiseMap;
    }
}
