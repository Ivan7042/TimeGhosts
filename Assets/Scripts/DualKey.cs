using UnityEngine;

public class DualKey : MonoBehaviour
{
    public DualKeyDoor linkedDoor;
    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player") || other.CompareTag("Ghost"))
        {
            isCollected = true;
            gameObject.SetActive(false);

            // Tell the door the current timer value when collected
            linkedDoor?.RegisterKeyCollected(this);
        }
    }

    public void ResetKey()
    {
        isCollected = false;
        gameObject.SetActive(true);

        // Ensure collider re-enabled
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}
