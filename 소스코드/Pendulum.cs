using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    IEnumerator Start()
    {
        float t = 0;
        yield return new WaitForSeconds(Random.Range(0, 0.5f));
        while (true)
        {
            t += Time.deltaTime * 2;
            transform.Rotate(0, 0, Mathf.Sin(t) * 0.45f);
            yield return null;
        }
    }
}
