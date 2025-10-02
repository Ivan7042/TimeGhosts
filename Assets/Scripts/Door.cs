using UnityEngine;

public class Door : MonoBehaviour
{

    private Collider doorCollider;
    private void Start()
    {
        doorCollider = GetComponent<Collider>();

        if (doorCollider == null)
        {
            doorCollider.isTrigger = false; // initially locked
        }
    }

    public void Unlock()
    {
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true; // make it a trigger to allow passage
        }
    }
}
