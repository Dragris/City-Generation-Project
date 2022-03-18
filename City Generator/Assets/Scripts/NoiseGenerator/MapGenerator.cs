using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode {NoiseMap, ColourMap};
    public DrawMode drawMode;
    public int mapWidth;
    public int mapHeight;

    [Range(0.1f, 20f)]
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public TerrainType[] regions;
    public float[,] noiseMap;

    public void GenerateMap() {
        noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float currentHeight = noiseMap [x, y];
                for (int i = 0; i < regions.Length; i++) {
                    // When we find the region we paint the color
                    // the region is found when current height is lower than region height,
                    // so regions have to be ordered from lower to higher
                    if (currentHeight <= regions [i].height){
                        colourMap[y * mapWidth + x] = regions [i].colour;
                        break;
                    }
                }
            }
        }
    
        MapDisplay display = FindObjectOfType<MapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColourMap) {
            display.DrawTexture (TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        
    }

    void OnValidate() {
        // Forces values to not drop below 1
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeight < 1) {
            mapHeight = 1;
        }
        if (lacunarity  < 1) {
            lacunarity = 1;
        }
        if (octaves < 1 ) {
            octaves = 1;
        }
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color colour;
    public bool buildable;
}