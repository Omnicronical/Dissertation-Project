using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable] public class BuildingType
{
    [SerializeField] private GameObject[] prefabs;
    public int sizeOfBuilding;
    public int numberOfBuildings;
    public int numberOfBuildingsPlaced;

    public GameObject SelectPrefab()
    {
        numberOfBuildingsPlaced++;
        if (prefabs.Length > 1 )
        {
            var random = UnityEngine.Random.Range(0, prefabs.Length);
            return prefabs[random];
        }
        return prefabs[0];
    }

    public bool CanPlaceBuilding()
    {
        return numberOfBuildingsPlaced < numberOfBuildings;
    }

    public void Reset()
    {
        numberOfBuildingsPlaced = 0;
    }

}
