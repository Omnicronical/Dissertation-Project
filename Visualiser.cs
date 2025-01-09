using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Visualiser : MonoBehaviour
{
    // Reference to the L-System string generator
    public StringGenerator lSystem;

    // List to store the positions of the agent
    List<Vector3> positions = new List<Vector3>();

    // References to external instantiators for roads, buildings, and trees
    public RoadInstantiater roadInstantiater;
    public BuildingInstantiater buildingInstantiater;
    public TreePlacement treePlacement;

    // Length of the segments drawn by the visualizer
    [SerializeField] public int length = 12;

    // Angle for turning during sequence visualization
    [SerializeField] private float angle = 90;

    // Array to store frame rates for performance measurement
    private List<int> frameRateArray = new List<int>();

    // Timer for tracking frame rate measurements
    private int timer = 0;

    // Stopwatch for measuring elapsed time
    Stopwatch stopwatch;

    // Property to ensure the length is always greater than 0
    public int Length
    {
        get
        {
            if (length > 0)
            {
                return length;
            }
            else
            {
                return 1;
            }
        }
        set => length = value;
    }

    private void Start()
    {
        // Start measuring time
        stopwatch = new Stopwatch();
        stopwatch.Start();

        // Generate the L-System sequence and visualize it
        var sequence = lSystem.GenerateSentence(2);
        VisualiseSequence(sequence);

        // Stop measuring time and log elapsed time
        stopwatch.Stop();
        UnityEngine.Debug.LogError("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    public void Regenerate(int lengthAdjustment, int iterations)
    {
        // Adjust the length and regenerate the visualized sequence
        length = lengthAdjustment;
        UnityEngine.Debug.LogError(length);

        // Reset all instantiated objects
        roadInstantiater.Reset();
        buildingInstantiater.Reset();
        treePlacement.ResetTrees();

        // Generate a new sequence and visualize it
        var sequence = lSystem.GenerateSentence(iterations);
        VisualiseSequence(sequence);
    }

    private void MeasureFrameRate()
    {
        // Calculate and log the average frame rate
        int totalFps = 0;

        foreach (int fps in frameRateArray)
        {
            totalFps += fps;
        }

        int averageFps = Mathf.RoundToInt(totalFps / frameRateArray.Count);

        UnityEngine.Debug.LogError("Average Frame Rate: " + averageFps);
    }

    //private void LateUpdate()
    //{
    //    if (timer <= 1200)
    //    {
    //        int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
    //        frameRateArray.Add(fps);
    //        timer += 1;
    //        UnityEngine.Debug.LogError(fps);
    //    }
    //    else
    //    {
    //        MeasureFrameRate();
    //    }
    //}

    private void VisualiseSequence(string sequence)
    {
        // Stack to store the agent's saved positions and directions
        Stack<AgentVariables> savePoints = new Stack<AgentVariables>();

        // Initialize the agent's starting position and direction
        var currentPosition = Vector3.zero;
        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

        // Add the starting position to the positions list
        positions.Add(currentPosition);

        // Loop through each character in the sequence and process it
        foreach (var letter in sequence)
        {
            EncodingLetters encoding = (EncodingLetters)letter;
            switch (encoding)
            {
                case EncodingLetters.save:
                    // Save the current position, direction, and length to the stack
                    savePoints.Push(new AgentVariables
                    {
                        position = currentPosition,
                        direction = direction,
                        length = Length
                    });
                    break;
                case EncodingLetters.load:
                    // Load the last saved position, direction, and length from the stack
                    if (savePoints.Count > 0)
                    {
                        var agentParameter = savePoints.Pop();
                        currentPosition = agentParameter.position;
                        direction = agentParameter.direction;
                        Length = agentParameter.length;
                    }
                    else
                    {
                        throw new System.Exception("Dont have saved point in the stack");
                    }
                    break;
                case EncodingLetters.draw:
                    // Move the agent forward and draw a road segment
                    tempPosition = currentPosition;
                    currentPosition += direction * length;
                    roadInstantiater.PlaceStreetAtPosition(tempPosition, Vector3Int.RoundToInt(direction), length);
                    Length -= 2;
                    positions.Add(currentPosition);
                    break;
                case EncodingLetters.turnRight:
                    // Rotate the direction clockwise
                    direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                    break;
                case EncodingLetters.turnLeft:
                    // Rotate the direction counter-clockwise
                    direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                    break;
                default:
                    break;
            }
        }

        // Finalize road placements, add buildings, and place trees
        roadInstantiater.FixRoad();
        buildingInstantiater.PlaceBuildingsOnRoad(roadInstantiater.GetRoadPositions());
        treePlacement.PlaceTrees();
    }

    // Enumeration for interpreting L-System commands
    public enum EncodingLetters
    {
        unknown = '1',    // Unknown command
        save = '[',       // Save the current state
        load = ']',       // Load the last saved state
        draw = 'F',       // Move forward and draw
        turnRight = '+',  // Turn clockwise
        turnLeft = '-'    // Turn counter-clockwise
    }
}
