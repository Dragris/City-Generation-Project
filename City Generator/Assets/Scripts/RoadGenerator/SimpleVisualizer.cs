using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lsystem;
    List<Vector3> positions = new List<Vector3>(); // List of positions our agent has travelled to
    public GameObject prefab;
    public Material lineMaterial;  // Line renderer from Unity (only to draw lines instead of using roads)

    public int initialLength = 8;
    public float angle = 90;  // As the prefab is using roads of 90 degrees it's the only option that we have
    [Range(0, 20)]
    public int lengthDelta =  2;

    public MapGenerator map;

    public int Length
    {
        get
        {
            if (initialLength > 0)
            {
                return initialLength;
            } else
            {
                return 1; // Length has to be higher than 0 to be visualized
            }
                
        }
        set => initialLength = value;
    }

    private void Start()
    {
        var sequence = lsystem.GenerateSentence();
        map.GenerateMap();
        VisualizeSequence(sequence);
    }

    private void VisualizeSequence(string sequence)
    {
        Stack<AgentParameters> savePoints = new Stack<AgentParameters>();
        var currentPosition = Vector3.zero;
        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

        positions.Add(currentPosition);

        foreach (var letter in sequence)
        {
            EncodingLetters encoding = (EncodingLetters)letter; // We need to be sure that each letter is inside of EncodingLetters enum, otherwise we don't recognise the order
            switch (encoding)
            {
                case EncodingLetters.save:
                    savePoints.Push(new AgentParameters
                    {
                        position = currentPosition,
                        direction = direction,
                        length = Length
                    });
                    break;

                case EncodingLetters.load:
                    if (savePoints.Count > 0)
                    {
                        // Gets last saved position only if there were one or more positions saved
                        var agentParameter = savePoints.Pop(); 
                        currentPosition = agentParameter.position;
                        direction = agentParameter.direction;
                        Length = agentParameter.length;
                    } else
                    {
                        throw new System.Exception("There's no saved point in the stack!");
                    }
                    break;

                case EncodingLetters.draw:
                    tempPosition = currentPosition;
                    currentPosition += direction * initialLength;  // Gives us the new point in the direction that we want.
                    DrawLine(tempPosition, currentPosition, Color.red);
                    Length -= lengthDelta; // We reduce the size of the line
                    positions.Add(currentPosition);
                    break;

                case EncodingLetters.turnRight:
                    direction = Quaternion.AngleAxis(angle, Vector3.up)*direction;
                    break;

                case EncodingLetters.turnLeft:
                    direction = Quaternion.AngleAxis(-angle, Vector3.up)*direction;
                    break;

                default:
                    throw new System.Exception("The command is not recognized.");
            }
        }

        // We spawn a point in each place our agent has visited.
        foreach (var position in positions)
        {
            if (Buildable(position.x, position.z) == true) Instantiate(prefab, position, Quaternion.identity);
        }
    }

    private void DrawLine(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        if (Buildable(startPoint.x, startPoint.z) == true && Buildable(endPoint.x, endPoint.z) == true) {
            GameObject line = new GameObject("line");
            line.transform.position = startPoint;
            var lineRenderer = line.AddComponent<LineRenderer>();
            // Give line its atributes
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            // Makes lines easier to see
            lineRenderer.SetWidth(1.25f, 1.25f);
            lineRenderer.SetPosition(0, startPoint); // Point number 0 of the line
            lineRenderer.SetPosition(1, endPoint);  // Point number 1 of the line
        } else {
            return;
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


    public enum EncodingLetters
    {
        unknown = '1',
        save = '[',
        load = ']',
        draw = 'F',
        turnRight = '+',
        turnLeft = '-'
    }
}
