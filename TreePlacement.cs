using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacement : MonoBehaviour
{
    public GameObject[] naturePrefabs;
    public Transform groundPlane;
    public LayerMask objectLayer; // Set this layer mask to the layer that your trees are on

    public int numberOfTrees = 10;
    public float radius = 10f;


    public void PlaceTrees()
    {
        for (int i = 0; i < numberOfTrees; i++)
        {
            Vector3 randomPoint = GetRandomPointOnGround();
            if (randomPoint != Vector3.zero && !IsObjectAtPoint(randomPoint))
            {
                Instantiate(naturePrefabs[UnityEngine.Random.Range(0, naturePrefabs.Length)], randomPoint, Quaternion.identity, transform);
            }
        }
    }

    Vector3 GetRandomPointOnGround()
    {
        Vector3 randomPoint = Vector3.zero;

        // Generate a random point within a radius
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        randomPoint = new Vector3(randomCircle.x, 0f, randomCircle.y);

        return randomPoint;
    }

    bool IsObjectAtPoint(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity, objectLayer))
        {
            return true; // Something is already there
        }
        return false; // Nothing found at the point
    }

    public void ResetTrees()
    {
        // Destroy all child objects of this GameObject
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
