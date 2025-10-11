using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DualKeyDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public DualKey keyA;
    public DualKey keyB;
    public CountdownTimer countdownTimer;
    public GameObject doorObject;

    [Header("UI Elements")]
    public TextMeshProUGUI keyCollectTimeText;
    public TextMeshProUGUI doorUnlockedText;
    public TextMeshProUGUI congratsText;
    public Button restartButton;

    private GamePhaseManager gamePhaseManager;
    private bool doorOpened = false;
    private bool playerReachedDoor = false;

    // Track key pickups (order-agnostic)
    private bool firstKeyCollected = false;
    private bool secondKeyCollected = false;
    private float firstKeyTime = -1f;
    private float secondKeyTime = -1f;

    // Reference to player controller or camera look script
    private CameraFollow playerLook;

    void Start()
    {
        gamePhaseManager = FindAnyObjectByType<GamePhaseManager>();
        playerLook = FindAnyObjectByType<CameraFollow>(); // or your camera control script

        if (keyA != null) keyA.linkedDoor = this;
        if (keyB != null) keyB.linkedDoor = this;

        if (keyCollectTimeText != null) keyCollectTimeText.text = "";
        if (doorUnlockedText != null) doorUnlockedText.gameObject.SetActive(false);
        if (congratsText != null) congratsText.gameObject.SetActive(false);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    public void RegisterKeyCollected(DualKey key)
    {
        if (doorOpened || countdownTimer == null) return;

        float currentTime = Mathf.CeilToInt(countdownTimer.GetTimeRemaining());

        if (keyCollectTimeText != null && gamePhaseManager != null && gamePhaseManager.IsPlaybackActive())
            keyCollectTimeText.text = $"Ghost collected key at: {currentTime}s";

        if (!firstKeyCollected)
        {
            firstKeyCollected = true;
            firstKeyTime = currentTime;
        }
        else if (!secondKeyCollected)
        {
            secondKeyCollected = true;
            secondKeyTime = currentTime;
        }

        CheckIfSynchronized();
    }

    private void CheckIfSynchronized()
    {
        if (!firstKeyCollected || !secondKeyCollected) return;

        if (Mathf.Approximately(firstKeyTime, secondKeyTime))
            Unlock();
        else
            ResetKeys();
    }

    private void Unlock()
    {
        doorOpened = true;

        Collider doorCollider = doorObject.GetComponent<Collider>();
        if (doorCollider != null)
            doorCollider.isTrigger = true;

        Debug.Log("Door unlocked!");

        if (countdownTimer != null)
            countdownTimer.StopTimer();

        if (doorUnlockedText != null)
            doorUnlockedText.gameObject.SetActive(true);

        // Don't show restart button yet — wait for player collision
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!doorOpened || playerReachedDoor) return;

        if (other.CompareTag("Player"))
        {
            playerReachedDoor = true;

            if (congratsText != null)
                congratsText.gameObject.SetActive(true);

            if (restartButton != null)
                restartButton.gameObject.SetActive(true);

            // Unlock cursor + disable camera/mouse look
            if (playerLook != null)
                playerLook.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Player reached unlocked door! Showing restart UI.");
        }
    }

    private void RestartGame()
    {
        Debug.Log("Restarting game...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetKeys()
    {
        firstKeyCollected = false;
        secondKeyCollected = false;
        firstKeyTime = -1f;
        secondKeyTime = -1f;

        if (keyA != null) keyA.ResetKey();
        if (keyB != null) keyB.ResetKey();
    }

    public void ResetDoor()
    {
        // Reset door state
        doorOpened = false;
        playerReachedDoor = false;

        // Reset the keys
        ResetKeys();

        // Reset door collider to be solid again
        if (doorObject != null)
        {
            doorObject.SetActive(true);
            var doorCol = doorObject.GetComponent<Collider>();
            if (doorCol != null)
                doorCol.isTrigger = false; // make the door solid again
        }

        // Hide all UI elements related to door/key
        if (doorUnlockedText != null)
            doorUnlockedText.gameObject.SetActive(false);

        if (congratsText != null)
            congratsText.gameObject.SetActive(false);

        if (restartButton != null)
            restartButton.gameObject.SetActive(false);

        if (keyCollectTimeText != null)
            keyCollectTimeText.text = "";

        // Optional: Reset any internal timers or state
        firstKeyCollected = false;
        secondKeyCollected = false;
        firstKeyTime = -1f;
        secondKeyTime = -1f;
    }

}
