using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    private void Awake()
    {   
        var obj = FindObjectsOfType<CanvasManager>();
        if (obj.Length == 1)
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);
    }
}
