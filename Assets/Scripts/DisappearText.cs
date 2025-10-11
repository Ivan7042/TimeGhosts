using UnityEngine;

public class DisappearText : MonoBehaviour
{
    public float disappearTime = 10f; 
    void Start()
    {
        Destroy(gameObject, disappearTime);
    }

}
