using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class labyrinth : MonoBehaviour
{
    public string csvFileName = "labyrinth.csv"; // Datei im StreamingAssets-Ordner
    public GameObject floorPrefab;
    public GameObject absorbierendPrefab;
    public GameObject transparentPrefab;
    public GameObject reflektierendPrefab;

    //  public GameObject horse;
    public GameObject ball;
    private GameObject wall;
    public GameObject target;

    private void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);
        string[] lines = File.ReadAllLines(path);

        for (int y = 0; y < lines.Length; y++)
        {
            string[] values = lines[y].Split(',');

            for (int x = 0; x < values.Length; x++)
            {
                int value = int.Parse(values[x]);
                Vector3 position = new Vector3(x, 0, -y);

                if (value != 0)
                {
                    position = new Vector3(x, 2, -y);
                }

                switch (value)
                {
                    case 0: //Boden, dort kann man gehen
                        wall = Instantiate(floorPrefab, position, Quaternion.identity);
                        break;
                    case 1: //Wand absorbierend
                        wall = Instantiate(absorbierendPrefab, position, Quaternion.identity);
                        break;
                    case 2: //Wand transparent
                        wall = Instantiate(transparentPrefab, position, Quaternion.identity);
                        break;
                    case 3: //Wand reflektierend
                        wall = Instantiate(reflektierendPrefab, position, Quaternion.identity);
                        break;
                    default:
                        Debug.LogWarning($"Unbekannter Wert {value} an Position ({x}, {y})");
                        break;
                }
                if (value != 0)
                {
                    wall.transform.localScale = new Vector3(1, 4, 1);
                }
            }
        }
        
        ball.transform.position = new Vector3(2, 1, -2f);
        ball.transform.Rotate(0, 180, 0);

        target.transform.position = new Vector3(34f,1,-35f);
    }
}

