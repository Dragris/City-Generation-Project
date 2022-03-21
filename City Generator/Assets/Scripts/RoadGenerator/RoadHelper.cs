using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadHelper : MonoBehaviour
{
    public GameObject roadStraight, roadCorner, road3Way, road4Way, roadEnd;
    Dictionary<Vector3Int, GameObject> roadDictionary = new Dictionary<Vector3Int, GameObject>();
    // This is to know if we need to change the prefab of the roads, to do corners
    HashSet<Vector3Int> fixRoadCandidates = new HashSet<Vector3Int>();
    MapGenerator map;

    public async void PlaceStreetPositions(Vector3 startPosition, Vector3Int direction, int length, MapGenerator map) {
        var rotation = Quaternion.identity;
        this.map = map;

        if (direction.x == 0) {
            // Rotates the road if it goes to the Z direction
            rotation = Quaternion.Euler(0, 90, 0);
        }

        for (int i = 0; i < length; i++) {
            // This gives the position to the next road
            var position = Vector3Int.RoundToInt(startPosition + direction * i);
            print(position);
            if (roadDictionary.ContainsKey(position)){
                continue;
            }
            
            if (!Buildable(position.x, position.z)) {
                // We break the loop of continuing the street
                fixRoadCandidates.Add(position); // Add the road to be fixed in a later pass
                return;
                // If we want to continue the street when the land becomes buildable again, use continue
                continue;
            }
            print("built");
            var road = Instantiate(roadStraight, position, rotation, transform);
            roadDictionary.Add(position, road);

            if (i == 0 ||i == length - 1) {
                fixRoadCandidates.Add(position);
            }
        }
    }

    public bool Buildable(float x, float y) {
            // Get current height to later find which region are we in
            // We truncate position to be in the same square
            print("X: " + x + " | Y: " + y);
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
            print("Buildable: " + buildableTerrain);    
            return buildableTerrain;
        }
}
