using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleVisualizer;

public class RoadVisualizer : MonoBehaviour
{
    public LSystemGenerator lsystem;
    List<Vector3> positions = new List<Vector3>(); // List of positions our agent has travelled to
    public RoadHelper roadHelper;

    public int lengthForNextPoint = 8;
    public float angle = 90;  // As the prefab is using roads of 90� it's the only option that we have
    [Range(0, 20)]
    public int lengthDelta =  2;

    public MapGenerator map;

    public int Length
    {
        get
        {
            if (lengthForNextPoint > 0)
            {
                return lengthForNextPoint;
            } else
            {
                return 1; // Length has to be higher than 0 to be visualized
            }
                
        }
        set => lengthForNextPoint = value;
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
                    currentPosition += direction * lengthForNextPoint;  // Gives us the new point in the direction that we want.
                    roadHelper.PlaceStreetPositions(tempPosition, Vector3Int.RoundToInt(direction), Length, this.map);
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

        
    }
}