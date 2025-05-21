using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ImageBehavior : MonoBehaviour
{
    private string imagepath = "Assets/Rendered_Images";
    private Transform parentTransform;
    private Vector3 basePosition;

    private Texture2D loadedTexture;
    private bool isViewing = false;
    
    private RawImage imageUI;
    private PlayerController playerController;

    void Start()
    {
        parentTransform = transform.parent;
        basePosition = parentTransform.position;
        var canvas = GameObject.Find("ImageCanvas");
        if (canvas != null)
        {
            imageUI = canvas.GetComponentInChildren<RawImage>(true); // ← `true` bedeutet: auch inaktive
        }
        
        playerController = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        
        LoadImage();
    }

    void Update()
    {
        if (!isViewing)
        {
            parentTransform.Rotate(0f, (50f * Time.deltaTime) % 360, 0f);
            float offsetY = Mathf.Sin(Time.time * 5) * 0.1f;
            parentTransform.position = basePosition + new Vector3(0, offsetY, 0);
        }
        else
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                HideImage();
            }
        }
    }

    void LoadImage()
    {
        Debug.Log("Starte Imageloader");

        if (!Directory.Exists(imagepath))
        {
            Debug.LogError($"Ordner nicht gefunden: {imagepath}");
            return;
        }

        string[] imagePaths = Directory
            .GetFiles(imagepath)
            .Where(f => f.ToLower().EndsWith(".png")
                     || f.ToLower().EndsWith(".jpg")
                     || f.ToLower().EndsWith(".jpeg"))
            .ToArray();

        if (imagePaths.Length == 0)
        {
            Debug.LogWarning("Keine Bilddateien im Ordner gefunden.");
            return;
        }

        string selectedPath = imagePaths[Random.Range(0, imagePaths.Length)];
        loadedTexture = LoadTextureFromPath(selectedPath);

        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = loadedTexture;
    }

    Texture2D LoadTextureFromPath(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(fileData);
        return tex;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter. Other Tag: " + other.tag);
        if (other.CompareTag("Player") && !isViewing)
        {
            ShowImage();
        }
    }

    void ShowImage()
    {
        Debug.Log(imageUI == null);
        Debug.Log(loadedTexture == null);
        if (imageUI == null || loadedTexture == null) return;

        imageUI.texture = loadedTexture;
        imageUI.gameObject.SetActive(true);
        isViewing = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (playerController != null)
            playerController.enabled = false;
    }

    void HideImage()
    {
        if (imageUI == null) return;

        imageUI.gameObject.SetActive(false);
        isViewing = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (playerController != null)
            playerController.enabled = true;

        gameObject.SetActive(false); // Objekt verschwindet nach Ansicht
    }
}
