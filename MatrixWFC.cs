using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixWFC : MonoBehaviour
{
    [SerializeField] public int width;
    [SerializeField] public int height;

    //This array is made up of 4 variables - 1 and 2 refer to the x an y coordinates within the map, 3 refers to the type of tile present and 4 is eother 0 or 1 which mean collapsed or notcollapsed respectively
    public int[,,] map;
    public List<Vector2Int> positionsToCollapse;
    public List<GameObject> NodePrefab = new List<GameObject>();

    public List<Tile> Tiles = new List<Tile>();

    private Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0,1), //Top
        new Vector2Int(0,-1), //Bottom
        new Vector2Int(1,0), //Right
        new Vector2Int(-1,0) //Left
    };

    void Start()
    {
        // Initialize the map array with dimensions based on width and height
        map = new int[width, height, 4];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y, 0] = x; // Assigning x coordinate
                map[x, y, 1] = y; // Assigning y coordinate
                map[x, y, 2] = (int)Nodes.Grass; // Setting type of tile to Grass initially
                map[x, y, 3] = 0;
            }
        }

        InitialiseTileRelationships();

        Collapse();

        SpawnMap();
    }

    void Collapse()
    {
        positionsToCollapse.Clear();

        int nodeCount = Enum.GetValues(typeof(Nodes)).Length;

        int midPointX = width / 2;
        int midPointY = height / 2;

        Vector2Int startingPosition = new Vector2Int(midPointX, midPointY);

        ////Collapse initial node then add the neighbours to the list

        //map[midPointX, midPointY, 2] = 4;
        ////map[midPointX, midPointY, 2] = UnityEngine.Random.Range(1, nodeCount);
        //map[midPointX, midPointY, 3] = 1;

        //for (int i = 0; i < offsets.Length; i++)
        //{
        //    Vector2Int neighbour = new Vector2Int(midPointX + offsets[i].x, midPointY + offsets[i].y);
        //    positionsToCollapse.Add(neighbour);
        //}

        positionsToCollapse.Add(startingPosition);
     
        while (positionsToCollapse.Count > 0) {

            Vector2Int position = positionsToCollapse[0];

            int x = position.x;
            int y = position.y;

            List<int> potentialNodes = new List<int>();


            for (int i = 0; i < nodeCount; i++)
            {
                potentialNodes.Add(i);
            }

            //Check each collapsed neighbour to see which nodes the neighbours cancel out
            //Iterate through neighbours
            for (int i = 0; i < offsets.Length; i++)
            {
                //Identify neighbour
                Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);

                if (IsInsideGrid(neighbour.x, neighbour.y))
                {

                    //Check whether the neighbour has been collapsed - if it has not it has no impact on the constraints and also does not need to be added to the to_Collapse
                    if (map[neighbour.x, neighbour.y, 3] == 1)
                    {

                        //Find tile number of neighbours - use this to create a reference for which class is needed for the relationships. 
                        int neighbourTileType = map[neighbour.x, neighbour.y, 2];

                        switch (i)
                        {
                            case 0:
                                CheckPossibleNodes(potentialNodes, neighbourTileType, i);
                                break;
                            case 1:
                                CheckPossibleNodes(potentialNodes, neighbourTileType, i);
                                break;
                            case 2:
                                CheckPossibleNodes(potentialNodes, neighbourTileType, i);
                                break;
                            case 3:
                                CheckPossibleNodes(potentialNodes, neighbourTileType, i);
                                break;
                        }



                    }
                    else
                    {
                        //Add uncollapsed neighbours to the list
                        if (!positionsToCollapse.Contains(neighbour)) positionsToCollapse.Add(neighbour);
                    }
                }
                
            }

            
            if (potentialNodes.Count > 0)
            {
                //Set the 3 value of the map piece to one of the potential nodes
                map[x, y, 2] = potentialNodes[UnityEngine.Random.Range(0, potentialNodes.Count-1)];

                //Set the 4 value of the map piece to 1
                map[x, y, 3] = 1;

            } else
            {
                
                //Set the current node to default tile type and DO NOT change to Collapsed
                map[x, y, 2] = (int)Nodes.Grass;

                //Find each neighbour
                for (int i = 0; i < offsets.Length; i++)
                {
                    Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
                    Debug.LogWarning(neighbour);
                    //Add the neighbours to the to collapse list if they are not already
                    if (!positionsToCollapse.Contains(neighbour))
                    {
                        if (IsInsideGrid(neighbour.x, neighbour.y))
                        { 
                            //Reset neighbours to default tile type

                            map[neighbour.x, neighbour.y, 2] = (int)Nodes.Grass;

                            //Reset neighbours to uncollapsed
                            map[neighbour.x, neighbour.y, 3] = 0;

                            positionsToCollapse.Add(neighbour);
                        }
                            
                    }
                }
                
                
            }
        
            

            //If there are no potential nodes then reset each neighbours 4 value to 0 and the 3 value to default as well
            //Add the current grid piece and the neighbours to the list if they are not already there. 

            //Must leave spaces of grass for buildings. 


            positionsToCollapse.RemoveAt(0);
        }

    }

    enum Nodes
    {
        Grass = 1,
        RoadHorizontal = 2,
        RoadVertical = 3,
        Junction = 4
    }

    void CheckPossibleNodes(List<int> potentialNodes, int tileType, int direction)
    {
        //Obtain neighbours class
        Debug.Log(tileType);
        Tile neighbourTileClass = Tiles[tileType];

        switch (direction)
        {
            case 0:
                //Find Top realtionships and cancel from potential nodes if they are not possible for the current node
                for (int i = potentialNodes.Count - 1; i > -1; i--)
                {
                    if (!neighbourTileClass.upRelations.Contains(potentialNodes[i]))
                    {
                        potentialNodes.RemoveAt(i);
                    }
                }
                break;
            case 1:
                //Find Down realtionships and cancel from potential nodes if they are not possible for the current node
                for (int i = potentialNodes.Count - 1; i > -1; i--)
                {
                    if (!neighbourTileClass.downRelations.Contains(potentialNodes[i]))
                    {
                        potentialNodes.RemoveAt(i);
                    }
                }
                break;
            case 2:
                //Find Right realtionships and cancel from potential nodes if they are not possible for the current node
                for (int i = potentialNodes.Count - 1; i > -1; i--)
                {
                    if (!neighbourTileClass.rightRelations.Contains(potentialNodes[i]))
                    {
                        potentialNodes.RemoveAt(i);
                    }
                }
                break;
            case 3:
                //Find Left realtionships and cancel from potential nodes if they are not possible for the current node
                for (int i = potentialNodes.Count - 1; i > -1; i--)
                {
                    if (!neighbourTileClass.leftRelations.Contains(potentialNodes[i]))
                    {
                        potentialNodes.RemoveAt(i);
                    }
                }
                break;

        }

    }

    void SpawnMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
               int xValue = map[x, y, 0];
               int yValue = map[x, y, 1];
               int TileType = map[x, y, 2];
               GameObject newNode = Instantiate(NodePrefab[TileType], new Vector3(xValue * 3f, 0f, yValue *3f), Quaternion.identity);
            }
        }
    }

    private bool IsInsideGrid(int x, int y)
    {
        if (x > -1 && x < width && y > -1 && y < height)
        {
            Debug.LogWarning(x);
            Debug.LogWarning(y);
            Debug.LogWarning("yes");
            return true;
        }
        else
        {
            return false;
        }
    }

    void InitialiseTileRelationships()
    {
        //In here set up each instance of the Tile class and add it to the Tiles list.
        Tile grass = new Tile();

        grass.tileNumber = (int)Nodes.Grass;

        grass.upRelations.Add(1);
        grass.upRelations.Add(2);
        grass.downRelations.Add(1);
        grass.downRelations.Add(2);
        grass.rightRelations.Add(1);
        grass.rightRelations.Add(3);
        grass.leftRelations.Add(1);
        grass.leftRelations.Add(3);

        grass.entropy = 2;

        Tiles.Add(grass);

        Tile roadHorizontal = new Tile();

        roadHorizontal.tileNumber = (int)Nodes.RoadHorizontal;

        roadHorizontal.upRelations.Add(1);
        roadHorizontal.upRelations.Add(2);
        roadHorizontal.downRelations.Add(1);
        roadHorizontal.downRelations.Add(2);
        roadHorizontal.rightRelations.Add(2);
        roadHorizontal.rightRelations.Add(4);
        roadHorizontal.leftRelations.Add(2);
        roadHorizontal.leftRelations.Add(4);

        roadHorizontal.entropy = 2;

        Tiles.Add(roadHorizontal);

        Tile roadVertical = new Tile();

        roadVertical.tileNumber = (int)Nodes.RoadVertical;

        roadVertical.upRelations.Add(3);
        roadVertical.upRelations.Add(4);
        roadVertical.downRelations.Add(3);
        roadVertical.downRelations.Add(4);
        roadVertical.rightRelations.Add(1);
        roadVertical.rightRelations.Add(2);
        roadVertical.leftRelations.Add(1);
        roadVertical.leftRelations.Add(2);

        roadVertical.entropy = 2;

        Tiles.Add(roadVertical);

        Tile junction = new Tile();

        junction.tileNumber = (int)Nodes.Junction;

        junction.upRelations.Add(3);
        junction.upRelations.Add(4);
        junction.downRelations.Add(3);
        junction.downRelations.Add(4);
        junction.rightRelations.Add(2);
        junction.rightRelations.Add(4);
        junction.leftRelations.Add(2);
        junction.leftRelations.Add(4);

        junction.entropy = 4;


        Tiles.Add(junction);

        Debug.Log(Tiles.Count);
    }

    // Add relationships for nodes -  class for each e number
    // Need to check the neighbours to assertain which node can be placed - use offsets
    // Then add entropy - a variable
 
    
    
    // multiple Arrays with the relationships in them as the form e numertors.
    // Rotation just means swapping over the arrays
}


//Insantitate multiple instances of this class in a list when the program starts. 
public class Tile 
{
    public int tileNumber = 0;

    public List<int> upRelations = new List<int>();
    public List<int> downRelations = new List<int>();
    public List<int> rightRelations = new List<int>();
    public List<int> leftRelations = new List<int>();

    public int entropy = 1;
}



