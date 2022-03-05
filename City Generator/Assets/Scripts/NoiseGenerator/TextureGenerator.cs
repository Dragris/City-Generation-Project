using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
        Texture2D texture = new Texture2D (width, height);
        // This makes the texture more crisp, can be deactivated
        texture.filterMode = FilterMode.Point;
        // This way we don't see the other side of the texture appearing
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels (colourMap);
        texture.Apply ();
        return texture;
    }

    public static Texture2D TextureFromHeightMap (float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        
        // First get all colours to apply them at the same time to all pixels
        // as it is more efficient that way
        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return TextureFromColourMap (colourMap, width, height);
    }
}
