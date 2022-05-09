using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureHelper : MonoBehaviour
{
    public BuildingType[] buildingTypes;
    public Dictionary<Vector3Int, GameObject> structuresDictionary = new Dictionary<Vector3Int, GameObject>();
    public int seed;

    public bool randomNaturePlacement = false;
    public GameObject[] naturePrefabs;
    [Range(0,1)]
    public float randomNaturePlacementThreshold = 0.3f;
    [Range(1, 4)]
    public int minimumNumberOfTreesPerPlot = 2;
    public int natureSeed;
    public Dictionary<Vector3, GameObject> natureDictionary = new Dictionary<Vector3, GameObject>();

    
    public void PlaceStructuresAroundRoad(List<Vector3Int> roadPositions, MapGenerator map) 
    {
        // Create prng to call the buildings
        System.Random prng = new System.Random(seed);

        // Create prng to call the "nature"
        System.Random prngNature = new System.Random(natureSeed);

        // Called from roadHelper
        Dictionary<Vector3Int, Direction> freeEstateSpots = FindFreeSpacesAroundRoad(roadPositions, map);
        List<Vector3Int> blockedPositions = new List<Vector3Int>(); 
        foreach (var freeSpot in freeEstateSpots)
        {
            // Check each freeSpot to see that there isn't anything in that space, such a big structure
            if (blockedPositions.Contains(freeSpot.Key)) continue;
            var rotation = Quaternion.identity;
            switch (freeSpot.Value)
            {   // All houses are pointing to the left
                case Direction.Up:
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case Direction.Down:
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Left:
                    break;
                case Direction.Right:
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                default:
                    break;
            }

            // Order of building types will go from the center of the road generation from the first element of the building types
            // Can be an issue, but can be exploited to position the biggest/highest houses in the center and then make suburbs
            for (int i = 0; i < buildingTypes.Length; i++)
            {
                // First place the bigger structures, then the smaller ones that can be continuously placed
                if (i == buildingTypes.Length -1)
                {
                        // Trees will only be placed when the "infinite" building is to be placed
                        if (randomNaturePlacement) {
                            float randomValue = (float)prngNature.Next(0, 10)/10f;
                            if (randomValue < randomNaturePlacementThreshold)
                            {
                                int numTrees = prngNature.Next(minimumNumberOfTreesPerPlot, 5);
                                for (int j = 0; j < numTrees; j++)
                                {
                                    float offSetX = prngNature.Next(-5, 5)/10f;
                                    float offsetZ = prngNature.Next(-5, 5)/10f;
                                    Vector3 treePosition = freeSpot.Key + new Vector3(offSetX, 0, offsetZ);
                                    // Try to regenerate if position already occupied
                                    if (natureDictionary.ContainsKey(treePosition))
                                    {
                                        j--;
                                        continue;
                                    }
                                    var treeRotation = Quaternion.Euler(0, prngNature.Next(0, 359), 0);

                                    var newTree = Instantiate(naturePrefabs[prngNature.Next(0, naturePrefabs.Length)], treePosition, treeRotation);
                                    natureDictionary.Add(treePosition, newTree);
                                }
                                
                                break;
                            }
                        }
                        var building = SpawnPrefab(buildingTypes[i].GetPrefab(prng.Next(0, buildingTypes[i].prefabs.Length)), freeSpot.Key, rotation);
                        structuresDictionary.Add(freeSpot.Key, building);
                        break;
                }
                print("Already placed = " + buildingTypes[i].quantityAlreadyPlaced);
                print("Amount of free real estate = " + freeEstateSpots.Count);
                print("Percentage = " + buildingTypes[i].quantity/100f);
                print("Amount I should want to place = " + freeEstateSpots.Count*(buildingTypes[i].quantity/100f));

                if (buildingTypes[i].quantityAlreadyPlaced < Mathf.CeilToInt(freeEstateSpots.Count*(buildingTypes[i].quantity/100f)))
                {
                    // If it has been placed less than enough times...
                    if (buildingTypes[i].sizeRequired > 1)
                    {
                        // Will make all estates to be an odd number of size
                        var halfSize = Mathf.FloorToInt(buildingTypes[i].sizeRequired/2.0f);
                        List<Vector3Int> tempPositionBlocked = new List<Vector3Int>();
                        if (VerifyIfBuildingFits(halfSize, freeEstateSpots, freeSpot, blockedPositions, ref tempPositionBlocked))
                        {
                            blockedPositions.AddRange(tempPositionBlocked);
                            var building = SpawnPrefab(buildingTypes[i].GetPrefab(prng.Next(0, buildingTypes[i].prefabs.Length)), freeSpot.Key, rotation);
                            structuresDictionary.Add(freeSpot.Key, building);
                            foreach(var pos in tempPositionBlocked)
                            {
                                structuresDictionary.Add(pos, building);
                            }
                            break;
                        }
                    } else {
                        var building = SpawnPrefab(buildingTypes[i].GetPrefab(prng.Next(0, buildingTypes[i].prefabs.Length)), freeSpot.Key, rotation);
                        structuresDictionary.Add(freeSpot.Key, building);
                    }
                    break;
                }
            }
        }
    }

    private Dictionary<Vector3Int, Direction> FindFreeSpacesAroundRoad(List<Vector3Int> roadPositions, MapGenerator map) 
    {
        Dictionary<Vector3Int, Direction> freeSpaces = new Dictionary<Vector3Int, Direction>();

        foreach(var position in roadPositions) 
        {
            var neighbourDirections = PlacementHelper.FindNeighbour(position, roadPositions);
            foreach(Direction direction in System.Enum.GetValues(typeof(Direction)))
            {
                if (neighbourDirections.Contains(direction) == false)
                {
                    var newPosition = position + PlacementHelper.GetOffsetFromDirection(direction);
                    bool buildable = Buildable(position.x, position.z, map) && Buildable(position.x+1.5f, position.z+1.5f, map)
                                    && Buildable(position.x+1.5f, position.z-1.5f, map) && Buildable(position.x-1.5f, position.z+1.5f, map)
                                    && Buildable(position.x-1.5f, position.z-1.5f, map);
                    if (freeSpaces.ContainsKey(newPosition) || !buildable)
                    { // If position already exists or not buildable
                        continue;
                    }
                    freeSpaces.Add(newPosition, PlacementHelper.GetReverseDirection(direction));
                }
            }
        }
        return freeSpaces;
    }

    private GameObject SpawnPrefab(GameObject prefab, Vector3Int position, Quaternion rotation)
    {
        var newStructure = Instantiate(prefab, position, rotation, transform);
        return newStructure;
    }

    private bool VerifyIfBuildingFits(int halfSize, Dictionary<Vector3Int, Direction> freeEstateSpots, KeyValuePair<Vector3Int, Direction> freeSpot, List<Vector3Int> blockedPositions, ref List<Vector3Int> tempPositionsBlocked)
    {
        Vector3Int direction = Vector3Int.zero;
        if(freeSpot.Value == Direction.Down || freeSpot.Value == Direction.Up)
        {   // Move to right or left to check the neighbours
            direction = Vector3Int.right;
        } else {
            // Move up or down to check the neighbours
            direction = new Vector3Int(0, 0, 1);
        }

        for (int i = 1; i <= halfSize; i++)
        {
            // Check both neighbours to see if they're free
            var pos1 = freeSpot.Key + direction*i;
            var pos2 = freeSpot.Key - direction*i;

            if(!freeEstateSpots.ContainsKey(pos1) || !freeEstateSpots.ContainsKey(pos2) ||
                blockedPositions.Contains(pos1) || blockedPositions.Contains(pos2))
            {   // Building does not fit. No need to check if buildable as freeEstateSpots have already checked that
                return false;
            }
            tempPositionsBlocked.Add(pos1);
            tempPositionsBlocked.Add(pos2);
        }
        return true;
    }

    public bool Buildable(float x, float y, MapGenerator map) {
            // Get current height to later find which region are we in
            // We truncate position to be in the same square
            float inMapPositionX = (map.mapWidth/2) - x/10;
            float inMapPositionY = (map.mapHeight/2) - y/10;
            float currentHeight = map.noiseMap [(int)inMapPositionX, (int)inMapPositionY];
            bool buildableTerrain = false;
            for (int i = 0; i < map.regions.Length; i++) {
                    // When we find the region we paint the color
                    // the region is found when current height is lower than region height,
                    // so regions have to be ordered from lower to higher
                    if (currentHeight <= map.regions [i].height){
                        buildableTerrain = map.regions[i].buildable;
                        break;
                    }
                }
            // If terrain is not buildable we skip positioning here anything  
            return buildableTerrain;
        }
}
