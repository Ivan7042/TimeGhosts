using UnityEngine;

public class ReplayableObject : MonoBehaviour
{
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public bool wasActiveInitially;

    protected virtual void Awake()
    {
        // Save initial state
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        wasActiveInitially = gameObject.activeSelf;

        // Register with GamePhaseManager
        var manager = FindAnyObjectByType<GamePhaseManager>();
        if (manager != null)
            manager.RegisterReplayable(this);
    }

    public virtual void ResetToInitialState()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        gameObject.SetActive(wasActiveInitially);
        
    }

    public virtual void OnGhostInteraction()
    {
        // Default: deactivate self (like being picked up)
        gameObject.SetActive(false);
    }
}
