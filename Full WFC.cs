// Necessary imports for Unity and system functionality
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BEAGSWFC : MonoBehaviour
{
    // Width and height of the grid to be generated
    [SerializeField] private int Width;
    [SerializeField] private int Height;

    // 2D array representing the grid of nodes
    private BEAGSNode[,] _grid;

    // List of all possible nodes available
    public List<BEAGSNode> Nodes = new List<BEAGSNode>();

    // List of grid positions that need to be collapsed
    private List<Vector2Int> _toCollapse = new List<Vector2Int>();

    // Flag to track placement errors during world collapse
    private bool PlacementError;

    // Layer mask to target objects in raycasting operations
    public LayerMask targetLayer;

    // Stores frame rates for performance measurement
    private List<int> frameRateArray = new List<int>();

    // Timer used for frame rate measurement
    private int timer = 0;

    // Stopwatch to measure elapsed time for world generation
    Stopwatch stopwatch;

    // Neighbor offsets for adjacent cells (up, down, left, right)
    private Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0,3), // Top
        new Vector2Int(0,-3), // Bottom
        new Vector2Int(3,0), // Right
        new Vector2Int(-3,0) // Left
    };

    // Neighbor offsets for diagonal cells (corners)
    private Vector2Int[] CornerOffsets = new Vector2Int[]
    {
        new Vector2Int(3,3), // TopRight
        new Vector2Int(3,-3), // BottomRight
        new Vector2Int(-3,3), // TopLeft
        new Vector2Int(-3,-3) // BottomLeft
    };

    // Reference to a specific type of node used in the grid
    public BEAGSNode Building;

    private void Start()
    {
        // Initialize and start the stopwatch for timing
        stopwatch = new Stopwatch();
        stopwatch.Start();

        // Initialize the grid with the specified dimensions
        _grid = new BEAGSNode[Width, Height];

        // Uncomment to initialize buildings (if required)
        // initialiseBuildings();

        // Begin collapsing the world to populate the grid
        CollapseWorld();

        // Stop the stopwatch and log the elapsed time
        stopwatch.Stop();
        UnityEngine.Debug.LogError("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
    }

    private void MeasureFrameRate()
    {
        int totalFps = 0;

        // Calculate the total frame rates stored in the array
        foreach (int fps in frameRateArray)
        {
            totalFps += fps;
        }

        // Compute and log the average frame rate
        int averageFps = Mathf.RoundToInt(totalFps / frameRateArray.Count);
        UnityEngine.Debug.LogError("Average Frame Rate: " + averageFps);
    }

    // Collapse the grid into a valid configuration
    private void CollapseWorld()
    {
        // Clear the list of positions to collapse
        _toCollapse.Clear();

        // Start collapsing from the center of the grid
        _toCollapse.Add(new Vector2Int(Width / 2, Height / 2));

        while (_toCollapse.Count > 0)
        {
            int x = _toCollapse[0].x; // Current x position
            int y = _toCollapse[0].y; // Current y position

            // List of potential nodes for the current position
            List<BEAGSNode> potentialNodes = new List<BEAGSNode>(Nodes);
            List<BEAGSNode> entropyNodes = new List<BEAGSNode>();

            // Adjust potential nodes based on their entropy values
            foreach (BEAGSNode node in potentialNodes)
            {
                if (node.Entropy != 0)
                {
                    for (int i = 0; i <= node.Entropy - 1; i++)
                    {
                        entropyNodes.Add(node);
                    }
                }
            }

            potentialNodes.AddRange(entropyNodes);

            // Analyze neighbors to whittle down valid node options
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);

                if (IsInsideGrid(neighbour))
                {
                    BEAGSNode neighbourNode = _grid[neighbour.x, neighbour.y];

                    if (neighbourNode != null)
                    {
                        switch (i)
                        {
                            case 0:
                                WhittleNodes(potentialNodes, neighbourNode.Bottom.CompatibleNodes);
                                break;
                            case 1:
                                WhittleNodes(potentialNodes, neighbourNode.Top.CompatibleNodes);
                                break;
                            case 2:
                                WhittleNodes(potentialNodes, neighbourNode.Left.CompatibleNodes);
                                break;
                            case 3:
                                WhittleNodes(potentialNodes, neighbourNode.Right.CompatibleNodes);
                                break;
                        }
                    }
                    else
                    {
                        // Add neighbor to collapse list if it's unprocessed
                        if (!_toCollapse.Contains(neighbour)) _toCollapse.Add(neighbour);
                    }
                }
            }

            // Handle cases where no compatible nodes are found
            if (potentialNodes.Count < 1)
            {
                _grid[x, y] = Nodes[0];
                PlacementError = true;
                UnityEngine.Debug.LogWarning($"Attempted to collapse wave on {x}, {y} but found no compatible nodes");

                for (int i = 0; i < offsets.Length; i++)
                {
                    Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                    if (IsInsideGrid(neighbour))
                    {
                        BEAGSNode neighbourNode = _grid[neighbour.x, neighbour.y];
                        if (neighbourNode != null && neighbourNode.HasBeenCollapsed && !_toCollapse.Contains(neighbour))
                        {
                            _toCollapse.Add(neighbour);
                        }
                    }
                }

                _toCollapse.Add(new Vector2Int(x, y));
            }
            else
            {
                // Randomly select a compatible node and assign it to the grid
                _grid[x, y] = potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];
            }

            // Instantiate the selected node at the grid position with rotation
            InstantiateNode(x, y);
            _toCollapse.RemoveAt(0);
        }
    }

    // Instantiates a node at a specific grid position
    private void InstantiateNode(int x, int y)
    {
        GameObject newNode;

        if (_grid[x, y].Rotated90)
        {
            newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x + 3, 0f, y), Quaternion.identity);
            newNode.transform.Rotate(0f, 90f, 0f, Space.Self);
        }
        else if (_grid[x, y].Rotated180)
        {
            newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x + 3, 0f, y - 3), Quaternion.identity);
            newNode.transform.Rotate(0f, 180f, 0f, Space.Self);
        }
        else if (_grid[x, y].Rotated270)
        {
            newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x, 0f, y - 3), Quaternion.identity);
            newNode.transform.Rotate(0f, 270f, 0f, Space.Self);
        }
        else
        {
            newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x, 0f, y), Quaternion.identity);
        }

        _grid[x, y].HasBeenCollapsed = true;
        _grid[x, y].InitialisedModel = newNode;
    }

    // Helper method to initialize buildings (if required)
    private void initialiseBuildings()
    {
        int buildingX = UnityEngine.Random.Range(1, Width);
        int buildingY = UnityEngine.Random.Range(1, Height);

        _grid[buildingX, buildingY] = Building;
        Instantiate(_grid[buildingX, buildingY].Prefab, new Vector3(buildingX - 1f, 0f, buildingY - 1f), Quaternion.identity);
    }

    // Removes nodes from the potential list that aren't in the valid nodes list
    private void WhittleNodes(List<BEAGSNode> potentialNodes, List<BEAGSNode> validNodes)
    {
        for (int i = potentialNodes.Count - 1; i > -1; i--)
        {
            if (!validNodes.Contains(potentialNodes[i]))
            {
                potentialNodes.RemoveAt(i);
            }
        }
    }

    // Checks if a given position is within the bounds of the grid
    private bool IsInsideGrid(Vector2Int v2int)
    {
        return v2int.x > -1 && v2int.x < Width && v2int.y > -1 && v2int.y < Height;
    }
}
