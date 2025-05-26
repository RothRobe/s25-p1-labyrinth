using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public GameObject winScreen;      // Das UI-Panel
    public TextMeshProUGUI winText;              // Der Text im Panel
    public float timeLimit = 120f;    // Zeit in Sekunden (2 Minuten)

    private float timer = 0f;
    private bool hasLost = false;
    public movement movement;

    void Update()
    {
        if (hasLost) return;

        timer += Time.deltaTime;

        if (timer >= timeLimit)
        {
            hasLost = true;
            ShowLoseScreen();
        }
    }

    void ShowLoseScreen()
    {
        winScreen.SetActive(true);
        winText.text = "Verloren";
        movement.setEnded(true);
    }
}