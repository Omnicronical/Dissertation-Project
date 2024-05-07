using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BEAGSWFC : MonoBehaviour
{
    [SerializeField] private int Width;
    [SerializeField] private int Height;

    private BEAGSNode[,] _grid;

    public List<BEAGSNode> Nodes = new List<BEAGSNode>();

    private List<Vector2Int> _toCollapse = new List<Vector2Int>();

    private bool PlacementError;

    public LayerMask targetLayer;

    private List<int> frameRateArray = new List<int>();

    private int timer = 0;

    Stopwatch stopwatch;

    private Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0,3), //Top
        new Vector2Int(0,-3), //Bottom
        new Vector2Int(3,0), //Right
        new Vector2Int(-3,0) //Left
    };

    private Vector2Int[] CornerOffsets = new Vector2Int[]
    {
        new Vector2Int(3,3), //TopRight
        new Vector2Int(3,-3), //BottomRight
        new Vector2Int(-3,3), //TopLeft
        new Vector2Int(-3,-3) //BottomLeft
    };

    public BEAGSNode Building;

    private void Start()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        _grid = new BEAGSNode[Width, Height];

        //initialiseBuildings();

        CollapseWorld();
        stopwatch.Stop();
        UnityEngine.Debug.LogError("Elapsed time: " + stopwatch.ElapsedMilliseconds + "ms");
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
    //    } else
    //    {
    //        MeasureFrameRate();
    //    }
        
    //}

    private void CollapseWorld()
    {
        _toCollapse.Clear();

        _toCollapse.Add(new Vector2Int(Width / 2, Height / 2));

        while (_toCollapse.Count > 0)
        {
            int x = _toCollapse[0].x;
            int y = _toCollapse[0].y;

            List<BEAGSNode> potentialNodes = new List<BEAGSNode>(Nodes);

            List<BEAGSNode> entropyNodes = new List<BEAGSNode>();

            //Adding aditional potential nodes based on their Entropy value
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

            foreach (BEAGSNode node in entropyNodes)
            {
                potentialNodes.Add(node);
            }

                

            for (int i = 0; i < offsets.Length; i++) {

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
                        if (!_toCollapse.Contains(neighbour)) _toCollapse.Add(neighbour);
                    }
                } //Can put code here to line the edges of the map with the sloped pieces
            }

            if (potentialNodes.Count < 1)
            {
                _grid[x, y] = Nodes[0];
                PlacementError = true;
                UnityEngine.Debug.LogWarning("Attempted to collapse wave on " + x + ", " + y + "but found no compatible nodes");

                // If nodes surrounding are not in the to collapse list then add them to the list
                //Could potentially add a variable that records if the node has been collapsed because there are two reasons that a node might not be in the list. 
                //(Should add this variable just to check)
                //(Not yet been added OR Collapsed)
                //(We dont want to add the "Not yet been added" nodes to the list)
                //I need to add the node being looked at to the to collapse list as well.

                for (int i = 0; i < offsets.Length; i++){

                    Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                    if (IsInsideGrid(neighbour)){
                        BEAGSNode neighbourNode = _grid[neighbour.x, neighbour.y];
                        if (neighbourNode != null && neighbourNode.HasBeenCollapsed) {
                            //Destroy GameObject
                            //Destroy(_grid[neighbour.x, neighbour.y].InitialisedModel);
                            //Add the Vector2Int of the neighbour to the _toCollapse list
                            //neighbourNode.HasBeenCollapsed = false;
                            if (!_toCollapse.Contains(neighbour)) _toCollapse.Add(neighbour);
                        }
                    }
                }

                //for (int i = 0; i < CornerOffsets.Length; i++){

                //    Vector2Int neighbour = new Vector2Int(x + CornerOffsets[i].x, y + CornerOffsets[i].y);
                //    if (IsInsideGrid(neighbour)){
                //        BEAGSNode neighbourNode = _grid[neighbour.x, neighbour.y];
                //        if (neighbourNode != null && neighbourNode.HasBeenCollapsed) {
                //            //Destroy GameObject
                //            //Destroy(_grid[neighbour.x, neighbour.y].InitialisedModel);
                //            //Add the Vector2Int of the neighbour to the _toCollapse list
                //            //neighbourNode.HasBeenCollapsed = false;
                //            if (!_toCollapse.Contains(neighbour)) _toCollapse.Add(neighbour);
                //        }
                //    }
                //}

                _toCollapse.Add(new Vector2Int(x, y));

                //Now we have all 8 neighbours of the node which we cannot collapse including the corners in an array - now we check if these nodes have been collapsed.
                //The ones which have not been collapsed are removed from the neighbourNodes list.
                //If they have then we destroy the prefab and then re-add them to the _toCollapse list.

            } else
            {
                _grid[x, y] = potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count)];
                
            }

            //Vector3 transform = new Vector3(x, 10f, y);

            //RaycastHit hit;
            //if (Physics.Raycast(transform, Vector3.down, out hit, 10f, targetLayer))
            //{
                // Check if the object hit is valid for deletion
                //if (hit.collider != null)
                //{
                    // Destroy the object
                    //Destroy(hit.collider.gameObject);
                    //Debug.Log("Hit" + x + y);
                //}
            //}

            if (_grid[x, y].Rotated90)
            {
                GameObject newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x + 3, 0f, y), Quaternion.identity);
                newNode.transform.Rotate(0f, 90f, 0f, Space.Self);
                _grid[x, y].HasBeenCollapsed = true;
                _grid[x, y].InitialisedModel = newNode;
                
                
                
            }
            else if (_grid[x, y].Rotated180)
            {
                GameObject newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x + 3, 0f, y - 3), Quaternion.identity);
                newNode.transform.Rotate(0f, 180f, 0f, Space.Self);
                _grid[x, y].HasBeenCollapsed = true;
                _grid[x, y].InitialisedModel = newNode;
               
                
                
            }
            else if (_grid[x, y].Rotated270)
            {
                GameObject newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x, 0f, y - 3), Quaternion.identity);
                newNode.transform.Rotate(0f, 270f, 0f, Space.Self);
                _grid[x, y].HasBeenCollapsed = true;
                _grid[x, y].InitialisedModel = newNode;
                
                
            }
            else
            {
                GameObject newNode = Instantiate(_grid[x, y].Prefab, new Vector3(x, 0f, y), Quaternion.identity);
                _grid[x, y].HasBeenCollapsed = true;
                _grid[x, y].InitialisedModel = newNode;
                
                
            } 

            _toCollapse.RemoveAt(0);

            //if (PlacementError = true){
                //Destroy(_grid[x, y].InitialisedModel);
                
            //}
            
            
            
        }
    }

    private void initialiseBuildings()
    {
        int buildingX = UnityEngine.Random.Range(1, Width);
        int buildingY = UnityEngine.Random.Range(1, Height);

        UnityEngine.Debug.Log(buildingX + buildingY);

        _grid[buildingX, buildingY] = Building;

        GameObject newNode = Instantiate(_grid[buildingX, buildingY].Prefab, new Vector3(buildingX-1f, 0f, buildingY-1f), Quaternion.identity);
    }

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

    private bool IsInsideGrid(Vector2Int v2int)
    {
        if (v2int.x > -1 && v2int.x < Width && v2int.y > -1 && v2int.y < Height)
        {
            return true;
        } else
        {
            return false;
        }
    }
}

