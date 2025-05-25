using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WallData
{
    public float x1;
    public float y1;
    public float x2;
    public float y2;
    public string type;
}

[System.Serializable]
public class WallList
{
    public List<WallData> walls;
}
public class MazeLoader : MonoBehaviour
{
    [Header("Dimensions")]
    public float scaleFactor;
    public float wallHeight;
    public float thickness;
    
    [Header("Generation Data")]
    public TextAsset jsonFile;
    public GameObject wallPrefab;          // Nur ein Prefab (z. B. Cube)
    public GameObject cylinderPrefab;
    public Material reflectiveMaterial;
    public Material absorbentMaterial;
    public Material transparentMaterial;

    void Start()
    {
        WallList wallList = JsonUtility.FromJson<WallList>(jsonFile.text);
        float yPos = wallHeight / 2;
        HashSet<(float, float)> columnPositionSet = new HashSet<(float, float)>();

        foreach (WallData wall in wallList.walls)
        {
            Vector3 start = new Vector3(wall.x1 * scaleFactor, yPos, wall.y1 * scaleFactor);
            Vector3 end = new Vector3(wall.x2 * scaleFactor, yPos, wall.y2 * scaleFactor);
            columnPositionSet.Add((wall.x1 * scaleFactor, wall.y1 * scaleFactor));
            columnPositionSet.Add((wall.x2 * scaleFactor, wall.y2 * scaleFactor));

            GameObject wallObj = CreateWall(start, end, wall.type);
        }
        foreach ((float, float) tupel in columnPositionSet)
        {
            GameObject columnObj = CreateColumn(new Vector3(tupel.Item1, 0, tupel.Item2));
        }

        GameObject roof = CreateRoof();
    }
    GameObject CreateWall(Vector3 start, Vector3 end, string type)
    {
        Vector3 center = (start + end) / 2f;
        Vector3 dir = end - start;
        float length = dir.magnitude;

        GameObject wall = Instantiate(wallPrefab, center, Quaternion.LookRotation(dir));

        // Wand skalieren
        wall.transform.localScale = new Vector3(thickness, wallHeight + thickness, length - thickness);

        // Material setzen

        Material mat = absorbentMaterial;
        if (type == "reflective") mat = reflectiveMaterial;
        else if (type == "transparent") mat = transparentMaterial;

        wall.GetComponent<Renderer>().material = mat;

        return wall;
    }

    GameObject CreateRoof()
    {
        Vector3 position = new Vector3(7f * scaleFactor / 2, wallHeight + thickness / 2, 7f * scaleFactor / 2);
        GameObject roof = Instantiate(wallPrefab, position, Quaternion.LookRotation(new Vector3(1, 0, 0)));
        roof.transform.localScale = new Vector3(7f * scaleFactor + thickness, thickness*4, 7f * scaleFactor + thickness);
        roof.GetComponent<Renderer>().material = absorbentMaterial;

        return roof;
    }

    GameObject CreateColumn(Vector3 position)
    {
        GameObject column = Instantiate(cylinderPrefab, position, Quaternion.LookRotation(new Vector3(1, 0, 0)));
        column.transform.localScale = new Vector3(thickness*2, wallHeight + thickness, thickness*2);
        column.GetComponent<Renderer>().material = absorbentMaterial;

        return column;
    }
}
