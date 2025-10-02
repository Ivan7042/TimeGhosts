using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float duration = 10f;

    private bool isRunning = false;
    private float timeRemaining;

    // Called externally to start countdown
    public void StartTimer(float customDuration = -1f)
    {
        timeRemaining = (customDuration > 0) ? customDuration : duration;
        isRunning = true;
        this.enabled = true;
    }

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            this.enabled = false; // disable ticking
        }

        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
    }
}
