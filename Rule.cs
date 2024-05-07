using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "LSystemProceduralCity/Rule")]

public class Rule : ScriptableObject
{
    public string letter;
    [SerializeField] private string[] results;
    [SerializeField] private bool randomResult = false;

    public string GetResults()
    {
        if (randomResult)
        {
            int randomIndex = UnityEngine.Random.Range(0, results.Length);
            return results[randomIndex];
        }
        return results[0];
    }
}
