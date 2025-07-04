using System.Collections;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NPCCommunication : MonoBehaviour
{
    [Header("KI API URL")]
    public string apiUrl = "http://localhost:5555"; // Hier die Uni-KI-URL eintragen

    [Header("Verweise")]
    public Transform npcTransform;
    public Transform playerTransform;

    [Header("Update-Intervall")]
    public float updateInterval = 10f;

    private bool initialized = false;

    private NPCController _npcController;

    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        _npcController = GetComponent<NPCController>();
        // Initial einmal das Maze und Startpositionen schicken
        string mazeData = GetMazeString();
        StartCoroutine(SendMazeInitToAI(mazeData));

        // Danach in regelmäßigen Abständen nur Statusupdates senden
        StartCoroutine(StatusUpdateLoop());
    }

    /// <summary>
    /// Holt dein Maze als String. Beispiel: Lies die maze.txt aus deinem MazeLoader.
    /// </summary>
    private string GetMazeString()
    {
        // TODO: Hier dein Maze als ASCII-String einfügen oder von deinem MazeLoader abrufen
        return string.Join("\n", File.ReadAllLines(GameObject.Find("MazeLoader").GetComponent<MazeLoader>().asciiFilePath));
    }

    private IEnumerator SendMazeInitToAI(string asciiMaze)
    {
        MazeInitData initData = new MazeInitData
        {
            maze = asciiMaze,
            npcPosition = GetPositionStruct(npcTransform),
            playerPosition = GetPositionStruct(playerTransform)
        };

        string jsonRequest = JsonUtility.ToJson(initData);

        yield return SendPostRequest(jsonRequest);

        initialized = true; // erst ab jetzt Statusupdates zulassen
    }

    private IEnumerator StatusUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (initialized)
            {
                StatusUpdateData statusData = new StatusUpdateData
                {
                    npcPosition = GetPositionStruct(npcTransform),
                    npcRotation = GetRotationStruct(npcTransform),
                    playerPosition = GetPositionStruct(playerTransform)
                };

                string jsonRequest = JsonUtility.ToJson(statusData);

                yield return SendPostRequest(jsonRequest);
            }
        }
    }

    private IEnumerator SendPostRequest(string jsonRequest)
    {
        Debug.Log(jsonRequest);
        using UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonRequest);

        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sende Request an KI...");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Fehler beim Senden: {request.error}");
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Antwort von KI: " + jsonResponse);

            try
            {
                ResponseData response = JsonUtility.FromJson<ResponseData>(jsonResponse);

                if (response != null)
                {
                    ProcessAIResponse(response);
                }
                else
                {
                    Debug.LogWarning("Keine gültige Antwort erhalten.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Fehler beim Parsen: {ex.Message}");
            }
        }
    }

    private void ProcessAIResponse(ResponseData response)
    {
        switch (response.action)
        {
            case "walk":
                Debug.Log($"NPC soll gehen zu: {response.target.x}, {response.target.y}, {response.target.z}");
                // npcController.WalkTo(...);
                _npcController.WalkTo(response.target.x, response.target.y, response.target.z);
                break;
            case "run":
                Debug.Log($"NPC soll rennen zu: {response.target.x}, {response.target.y}, {response.target.z}");
                _npcController.RunTo(response.target.x, response.target.y, response.target.z);
                break;
            case "crawl":
                Debug.Log($"NPC soll kriechen zu: {response.target.x}, {response.target.y}, {response.target.z}");
                _npcController.CrawlTo(response.target.x, response.target.y, response.target.z);
                break;
            case "cry":
                Debug.Log("NPC soll weinen.");
                _npcController.StartCrying();
                break;
            case "idle":
                Debug.Log("NPC soll idle bleiben.");
                break;
            default:
                Debug.LogWarning($"Unbekannte Aktion: {response.action}");
                break;
        }
    }

    private Position GetPositionStruct(Transform t) => new Position
    {
        x = t.position.x,
        y = t.position.y,
        z = t.position.z
    };

    private Rotation GetRotationStruct(Transform t) => new Rotation
    {
        x = t.eulerAngles.x,
        y = t.eulerAngles.y,
        z = t.eulerAngles.z
    };
}

[System.Serializable]
public class MazeInitData
{
    public string maze;
    public Position npcPosition;
    public Position playerPosition;
}

[System.Serializable]
public class StatusUpdateData
{
    public Position npcPosition;
    public Rotation npcRotation;
    public Position playerPosition;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Rotation
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ResponseData
{
    public string action;
    public TargetPosition target;
}

[System.Serializable]
public class TargetPosition
{
    public int x;
    public int y;
    public int z;
}
