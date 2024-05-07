using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingInstantiater : MonoBehaviour
{
    public BuildingType[] buildingTypes;
    public GameObject[] naturePrefabs;

    public bool randomNaturePlacement = true;
    [Range(0, 1)] public float naturePlacementThreshold = 0.3f;

    public Dictionary<Vector3Int, GameObject> buildingsDictionary = new Dictionary<Vector3Int, GameObject>();
    public Dictionary<Vector3Int, GameObject> natureDictionary = new Dictionary<Vector3Int, GameObject>();

    public void PlaceBuildingsOnRoad(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Direction> freeForPlacement = FindFreeSpaces(roadPositions);
        List<Vector3Int> blockedPositions = new List<Vector3Int>();

        foreach (var freePosition in freeForPlacement) 
        {
            if (blockedPositions.Contains(freePosition.Key))
            {
                continue;
            }

            var rotation = Quaternion.identity;

            switch (freePosition.Value)
            {
                case Direction.Down:
                    rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Direction.Left:
                    rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Right:
                    rotation = Quaternion.Euler(0, 90, 0);
                    break;
                default:
                    break;
            }
            for (int i = 0; i < buildingTypes.Length; i++)
            {
                if (buildingTypes[i].numberOfBuildings == -1)
                {
                    if (randomNaturePlacement)
                    {
                        var random = UnityEngine.Random.value;
                        if(random < naturePlacementThreshold)
                        {
                            var nature = SpawnPrefab(naturePrefabs[UnityEngine.Random.Range(0, naturePrefabs.Length)], freePosition.Key, Quaternion.identity);
                            natureDictionary.Add(freePosition.Key, nature);
                            break;
                        }
                    }
                    var building = SpawnPrefab(buildingTypes[i].SelectPrefab(), freePosition.Key, rotation);
                    building.layer = 3;
                    buildingsDictionary.Add(freePosition.Key, building);
                    break;
                }
                if (buildingTypes[i].CanPlaceBuilding())
                {
                    if (buildingTypes[i].sizeOfBuilding > 1)
                    {
                        Debug.Log("Yes");
                        var halfSize = Mathf.FloorToInt(buildingTypes[i].sizeOfBuilding / 2.0f);
                        List<Vector3Int> tempPositionsBlocked = new List<Vector3Int>();
                        if (CheckBuildingFits(halfSize, freeForPlacement, freePosition, blockedPositions,  ref tempPositionsBlocked))
                        {
                            blockedPositions.AddRange(tempPositionsBlocked);
                            var building = SpawnPrefab(buildingTypes[i].SelectPrefab(), freePosition.Key, rotation);
                            building.layer = 3;
                            buildingsDictionary.Add(freePosition.Key, building);
                            foreach (var position in tempPositionsBlocked)
                            {
                                buildingsDictionary.Add(position, building);
                            }
                           
                        }
                    }
                    else
                    {
                        var building = SpawnPrefab(buildingTypes[i].SelectPrefab(), freePosition.Key, rotation);
                        building.layer = 3;
                        buildingsDictionary.Add(freePosition.Key, building);
                       
                    }
                    break;
                }
            }
            
        }
    }

    private bool CheckBuildingFits
        (int halfSize,
        Dictionary<Vector3Int, Direction> freeForPlacement,
        KeyValuePair<Vector3Int, Direction> freePosition,
        List<Vector3Int> blockedPositions,
        ref List<Vector3Int> tempPositionsBlocked)
    {
        Vector3Int direction = Vector3Int.zero;
        if (freePosition.Value == Direction.Down || freePosition.Value == Direction.Up)
        {
            direction = Vector3Int.right;
        }
        else
        {
            direction = new Vector3Int(0, 0, 1);
        }
        for (int i = 1; i <= halfSize; i++)
        {
            var positionOne = freePosition.Key + direction * i;
            var positionTwo = freePosition.Key - direction * i;
            if (!freeForPlacement.ContainsKey(positionOne) || !freeForPlacement.ContainsKey(positionTwo) ||
                blockedPositions.Contains(positionOne) || blockedPositions.Contains(positionTwo))
            {
                return false;
            }
            tempPositionsBlocked.Add(positionOne);
            tempPositionsBlocked.Add(positionTwo);
        }
        return true;
    }

    private GameObject SpawnPrefab(GameObject prefab, Vector3Int position, Quaternion rotation)
    {
        var newBuilding = Instantiate(prefab, position, rotation, transform);
        return (newBuilding);
    }

    private Dictionary<Vector3Int, Direction> FindFreeSpaces(List<Vector3Int> roadPositions)
    {
        Dictionary<Vector3Int, Direction> freeSpaces = new Dictionary<Vector3Int, Direction>();
        foreach (var position in roadPositions)
        {
            var neighbourDirections = PlacementModifier.FindNeighbour(position, roadPositions);
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (neighbourDirections.Contains(direction) == false)
                {
                    var newPosition = position + PlacementModifier.GetOffsetFromDirection(direction);
                    if (freeSpaces.ContainsKey(newPosition))
                    {
                        continue;
                    }
                    freeSpaces.Add(newPosition, PlacementModifier.GetReverseDirection(direction));
                }
            }
        }
        return freeSpaces;
        
    }

    public void Reset()
    {
        foreach (var building in buildingsDictionary.Values)
        {
            Destroy(building);
        }

        foreach (var nature in natureDictionary.Values)
        {
            Destroy(nature);
        }

        foreach (var typeOfBuilding in buildingTypes)
        {
            typeOfBuilding.numberOfBuildingsPlaced = 0;
        }

        natureDictionary.Clear();
        buildingsDictionary.Clear();
    }
}
