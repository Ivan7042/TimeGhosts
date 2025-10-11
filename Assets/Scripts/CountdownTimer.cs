using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float duration = 10f;

    private bool isRunning = false;
    private float timeRemaining;

    // Start the timer (optional custom duration)
    public void StartTimer(float customDuration = -1f)
    {
        timeRemaining = (customDuration > 0) ? customDuration : duration;
        isRunning = true;
        this.enabled = true;
        UpdateDisplay();
    }

    // Stop the timer (pause)
    public void StopTimer()
    {
        isRunning = false;
        this.enabled = false;
    }

    // Fully reset timer display
    public void ResetTimer()
    {
        StopTimer();
        timeRemaining = 0f;
        UpdateDisplay();
    }

    public float GetTimeRemaining() => timeRemaining;

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

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        if (timerText != null)
            timerText.text = seconds.ToString();
    }
}
