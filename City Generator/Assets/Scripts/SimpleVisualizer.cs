using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleVisualizer : MonoBehaviour
{
    public LSystemGenerator lsystem;
    List<Vector3> positions = new List<Vector3>(); // List of positions our agent has travelled to
    public GameObject prefab;
    public Material lineMaterial;  // Line renderer from Unity (only to draw lines instead of using roads)

    public int lengthForNextPoint = 8;
    private float angle = 90;  // As the prefab is using roads of 90� it's the only option that we have

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
                    DrawLine(tempPosition, currentPosition, Color.red);
                    Length -= 2; // We reduce the size of the line
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
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    private void DrawLine(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        GameObject line = new GameObject("line");
        line.transform.position = startPoint;
        var lineRenderer = line.AddComponent<LineRenderer>();
        // Give line its atributes
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(0, startPoint); // Point number 0 of the line
        lineRenderer.SetPosition(1, endPoint);  // Point number 1 of the line
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
