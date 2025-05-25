using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntArray
{
    public int[] array;
}

[System.Serializable]
public class LabyrinthPlan
{
    public float wallHeight;
    public float squareSize;
    public IntArray[] wallsX;
    public IntArray[] wallsZ;
}

public class BuildLabyrinth : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject pillarPrefab;
    public TextAsset planJson;

    public Material absorbentMaterial;   // 1
    public Material reflectiveMaterial;  // 2
    public Material transparentMaterial; // 3

    void Start()
    {
        if (planJson == null)
        {
            Debug.LogError("Plan JSON not assigned!");
            return;
        }
        LabyrinthPlan plan = JsonUtility.FromJson<LabyrinthPlan>(planJson.text);

        // X-Wände (horizontal)
        BuildWalls(plan.wallsX, plan, true);

        // Z-Wände (vertikal)
        BuildWalls(plan.wallsZ, plan, false);
    }

    void BuildWalls(IntArray[] wallArrays, LabyrinthPlan plan, bool isX)
    {
        for (int z = 0; z < wallArrays.Length; z++)
        {
            for (int x = 0; x < wallArrays[z].array.Length; x++)
            {
                // Pillars
                if (isX)
                {
                    Vector3 pillarPos = new Vector3(x * plan.squareSize - 0.1f * plan.squareSize, 0, z * plan.squareSize);
                    // Create a pillar at the end of the wall
                    GameObject pillar = Instantiate(pillarPrefab, pillarPos, Quaternion.Euler(0, 90, 0), transform);
                    pillar.transform.localScale = new Vector3(plan.squareSize, plan.wallHeight, plan.squareSize);
                }

                // Empty wall check
    	        int mat = wallArrays[z].array[x];
                if (mat == 0) continue;

                // Create wall
                Vector3 pos;
                Quaternion rot;
                if (isX)
                {
                    pos = new Vector3(x * plan.squareSize - 0.1f * plan.squareSize, 0, z * plan.squareSize);
                    rot = Quaternion.identity;
                }
                else
                {
                    pos = new Vector3(x * plan.squareSize + 0.1f * plan.squareSize, 0, z * plan.squareSize);
                    rot = Quaternion.Euler(0, 90, 0);

                }

                GameObject wall = Instantiate(wallPrefab, pos, rot, transform);
                wall.transform.localScale = new Vector3(plan.squareSize, plan.wallHeight, 0.8f * plan.squareSize);
                wall.name = (isX ? $"WallX_{x}_{z}_{mat}" : $"WallZ_{x}_{z}_{mat}");
                var renderer = wall.GetComponent<Renderer>();
                Material material = GetMaterial(mat);
                if (renderer != null && material != null)
                {
                    renderer.material = material;
                }
            }
        }
    }

    Material GetMaterial(int mat)
    {
        switch (mat)
        {
            case 1: return absorbentMaterial;
            case 2: return reflectiveMaterial;
            case 3: return transparentMaterial;
            default: return null;
        }
    }
}