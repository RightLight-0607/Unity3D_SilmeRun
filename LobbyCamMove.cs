using UnityEngine;

public class LobbyCamMove : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles += transform.up * 0.01f;
    }
}
