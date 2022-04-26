using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadHelper : MonoBehaviour
{
    public GameObject roadStraight, roadCorner, road3Way, road4Way, roadEnd;
    Dictionary<Vector3Int, GameObject> roadDictionary = new Dictionary<Vector3Int, GameObject>();
    // This is to know if we need to change the prefab of the roads, to do corners
    HashSet<Vector3Int> fixRoadCandidates = new HashSet<Vector3Int>();

    public List<Vector3Int> GetRoadPositions()
    {
        return roadDictionary.Keys.ToList();
    }

    MapGenerator map;

    public async void PlaceStreetPositions(Vector3 startPosition, Vector3Int direction, int length, MapGenerator map) {
        var rotation = Quaternion.identity;
        this.map = map;

        if (direction.x == 0) {
            // Rotates the road if it goes to the Z direction
            rotation = Quaternion.Euler(0, 90, 0);
        }
        var position = new Vector3Int(0,0,0);
        for (int i = 0; i < length; i++) {
            // This gives the position to the next road
            var previousposition = position;
            position = Vector3Int.RoundToInt(startPosition + direction * i);
            print(position);
            if (roadDictionary.ContainsKey(position)){
                continue;
            }
            
            if (!Buildable(position.x, position.z) || !Buildable(position.x+1, position.z)
            || !Buildable(position.x-1, position.z) || !Buildable(position.x, position.z+1)
            || !Buildable(position.x, position.z-1)) {
                // We break the loop of continuing the street
                if (i != 0) fixRoadCandidates.Add(previousposition); // Add the road to be fixed in a later pass
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

    public void FixRoad()
    {
        // Iterate over each road candidate to fix
        foreach (var position in fixRoadCandidates)
        {
            List<Direction> neighbourDirections = PlacementHelper.FindNeighbour(position, roadDictionary.Keys);

            Quaternion rotation = Quaternion.identity;
            // Don't call destroy before as there is a statement where it is not necessary
            if (neighbourDirections.Count == 1) {
                // Road end
                Destroy(roadDictionary[position]);

                // Get rotation for road end, default rotation is Right
                if (neighbourDirections.Contains(Direction.Down)) rotation = Quaternion.Euler(0,90,0);
                if (neighbourDirections.Contains(Direction.Left)) rotation = Quaternion.Euler(0,180,0);
                if (neighbourDirections.Contains(Direction.Up)) rotation = Quaternion.Euler(0,270,0);

                roadDictionary[position] = Instantiate(roadEnd, position, rotation, transform);

            } else if (neighbourDirections.Count == 2) {
                if (neighbourDirections.Contains(Direction.Up) && neighbourDirections.Contains(Direction.Down) ||
                neighbourDirections.Contains(Direction.Right) && neighbourDirections.Contains(Direction.Left)) {
                    // This is still a straight road, just continue
                    continue;
                } else {
                    // Place a corner, default is Left and Up
                    Destroy(roadDictionary[position]);
                    if (neighbourDirections.Contains(Direction.Up) && neighbourDirections.Contains(Direction.Right)) rotation = Quaternion.Euler(0,90,0);
                    if (neighbourDirections.Contains(Direction.Right) && neighbourDirections.Contains(Direction.Down)) rotation = Quaternion.Euler(0,180,0);
                    if (neighbourDirections.Contains(Direction.Down) && neighbourDirections.Contains(Direction.Left)) rotation = Quaternion.Euler(0,270,0);

                    roadDictionary[position] = Instantiate(roadCorner, position, rotation, transform);
                }
            } else if (neighbourDirections.Count == 3) {
                // Place the 3 way, default is to Up, Right and Down
                Destroy(roadDictionary[position]);
                
                if (neighbourDirections.Contains(Direction.Right) && neighbourDirections.Contains(Direction.Down) && neighbourDirections.Contains(Direction.Left)) rotation = Quaternion.Euler(0,90,0);
                if (neighbourDirections.Contains(Direction.Down) && neighbourDirections.Contains(Direction.Left) && neighbourDirections.Contains(Direction.Up)) rotation = Quaternion.Euler(0,180,0);
                if (neighbourDirections.Contains(Direction.Left) && neighbourDirections.Contains(Direction.Up) && neighbourDirections.Contains(Direction.Right)) rotation = Quaternion.Euler(0,270,0);

                roadDictionary[position] = Instantiate(road3Way, position, rotation, transform);
            } else {
                // 4 Way split
                Destroy(roadDictionary[position]);
                roadDictionary[position] = Instantiate(road4Way, position, rotation, transform);
            }
        }
    }

    public bool Buildable(float x, float y) {
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
