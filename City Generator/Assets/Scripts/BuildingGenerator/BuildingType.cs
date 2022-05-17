using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class BuildingType
{
    [SerializeField]
    public GameObject[] prefabs; // Here we store all the prefabs that are going to be used
    [Min(0)]
    public int sizeRequired; // Size that the structure requires to be placed

    [Range(0, 100)]
    public float quantity;
    public int quantityAlreadyPlaced; // Count of structures placed on the map
    

    public GameObject GetPrefab(int pos) 
    {
        quantityAlreadyPlaced++;
        if (prefabs.Length > 1)
        {
            // Get random building from already created prng
            return prefabs[pos];
        }
        return prefabs[0];
    }

    public void Reset()
    {
        quantityAlreadyPlaced = 0;
    }
}
