using System.Collections;
using UnityEngine;

public class Lift : MonoBehaviour
{
    [SerializeField] Transform startPos;
    [SerializeField] Transform endPos;
    IEnumerator Start()
    {
        float t = 0;
        float speed = 0.1f;
        while (true)
        {
            t += Time.deltaTime * speed;
            if(t > 1 || t < 0)
            {
                speed = -speed;
            }
            transform.position = Vector3.Lerp(startPos.position, endPos.position, t);
            yield return null;
        }
    }
}
