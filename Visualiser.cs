using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Visualiser : MonoBehaviour
{
    public StringGenerator lSystem;
    List<Vector3> positions = new List<Vector3>();
    public RoadInstantiater roadInstantiater;
    public BuildingInstantiater buildingInstantiater;
    public TreePlacement treePlacement;

    [SerializeField] public int length = 12;
    [SerializeField] private float angle = 90;

    private List<int> frameRateArray = new List<int>();

    private int timer = 0;

    Stopwatch stopwatch;

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
        stopwatch = new Stopwatch();
        stopwatch.Start();
        var sequence = lSystem.GenerateSentence(2);
        VisualiseSequence(sequence);
        stopwatch.Stop();
        UnityEngine.Debug.LogError("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    public void Regenerate(int lengthAdjustment, int iterations)
    {
        length = lengthAdjustment;
        UnityEngine.Debug.LogError(length);
        roadInstantiater.Reset();
        buildingInstantiater.Reset();
        treePlacement.ResetTrees();
        var sequence = lSystem.GenerateSentence(iterations);
        VisualiseSequence(sequence);
    }

    private void MeasureFrameRate()
    {
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
        Stack<AgentVariables> savePoints = new Stack<AgentVariables>();
        var currentPosition = Vector3.zero;

        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

        positions.Add(currentPosition);

        foreach (var letter in sequence)
        {
            EncodingLetters encoding = (EncodingLetters)letter;
            switch (encoding)
            {
                case EncodingLetters.save:
                    savePoints.Push(new AgentVariables
                    {
                        position = currentPosition,
                        direction = direction,
                        length = Length
                    });
                    break;
                case EncodingLetters.load:
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
                    tempPosition = currentPosition;
                    currentPosition += direction * length;
                    roadInstantiater.PlaceStreetAtPosition(tempPosition, Vector3Int.RoundToInt(direction), length);
                    Length -= 2;
                    positions.Add(currentPosition);
                    break;
                case EncodingLetters.turnRight:
                    direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                    break;
                case EncodingLetters.turnLeft:
                    direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;
                    break;
                default:
                    break;
            }
        }
        roadInstantiater.FixRoad();
        buildingInstantiater.PlaceBuildingsOnRoad(roadInstantiater.GetRoadPositions());
        treePlacement.PlaceTrees();
        
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

