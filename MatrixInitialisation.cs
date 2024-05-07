using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixPopulator : MonoBehaviour
{
    // Enum for tile types
    enum TileType
    {
        Grass,
        Vegetation,
        Road,
        Houses
    }

    // Enum for direction
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    // Function to populate the matrix
    TileType[,] PopulateMatrix(int N)
    {
        TileType[,] matrix = new TileType[N, N];

        // Populate the matrix with grass initially
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                matrix[i, j] = TileType.Grass;
            }
        }

        // Place roads
        for (int i = 0; i < N; i++)
        {
            matrix[i, N / 2] = TileType.Road;
            matrix[N / 2, i] = TileType.Road;
        }

        // Place houses or vegetation
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (matrix[i, j] != TileType.Road)
                {
                    if ((i + j) % 2 == 0)
                        matrix[i, j] = TileType.Houses;
                    else
                        matrix[i, j] = TileType.Vegetation;
                }
            }
        }

        return matrix;
    }

    // Function to orientate the tiles
    void OrientateTiles(TileType[,] matrix)
    {
        int N = matrix.GetLength(0);
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (matrix[i, j] == TileType.Houses || matrix[i, j] == TileType.Road)
                {
                    if (i > 0 && matrix[i - 1, j] == matrix[i, j]) // Check up
                        Debug.Log($"Tile at ({i}, {j}) oriented upwards");
                    else if (i < N - 1 && matrix[i + 1, j] == matrix[i, j]) // Check down
                        Debug.Log($"Tile at ({i}, {j}) oriented downwards");
                    else if (j > 0 && matrix[i, j - 1] == matrix[i, j]) // Check left
                        Debug.Log($"Tile at ({i}, {j}) oriented leftwards");
                    else if (j < N - 1 && matrix[i, j + 1] == matrix[i, j]) // Check right
                        Debug.Log($"Tile at ({i}, {j}) oriented rightwards");
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        int N = 5; // Size of the matrix
        TileType[,] matrix = PopulateMatrix(N);

        // Output the matrix
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                Debug.Log(matrix[i, j] + " ");
            }
            Debug.Log("\n");
        }

        Debug.Log("\nOrientation of tiles:");
        OrientateTiles(matrix);
    }
}
