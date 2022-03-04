using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000,100000) + offset.x;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Making sure it's not 0 so the division does not crash, even if this is controlled with the range
        if (scale <=  0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Used later to zoom in and out from center of plane instead of a corner
        float halfWidth = mapWidth/2f;
        float halfHeight = mapHeight/2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth)/ scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight)/ scale * frequency + octaveOffsets[i].y;

                    // Multiplied and substracted to get negative values so that we can have
                    // decreases in height
                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 -1;
                    noiseHeight += perlinValue * amplitude;
                    noiseMap [x, y] = perlinValue;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x,y] = noiseHeight;
            }
            
        }
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                // Basically normalises again all the noisemap to be again between 0 and 1
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
