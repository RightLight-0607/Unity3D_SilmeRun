using System.Collections;
using UnityEngine;

public class StunEffect : MonoBehaviour
{
    [SerializeField] GameObject[] stars = new GameObject[2];
    [SerializeField] GameObject player;
    float speed = 10f;
    private void OnEnable()
    {
        StartCoroutine(SetObject());
    }
    void Update()
    {
        transform.eulerAngles += Vector3.up * 0.1f * speed;
        for (int i = 0; i < 2; i++)
        {
            stars[i].transform.localEulerAngles = player.transform.eulerAngles - transform.eulerAngles;
        }
    }
    IEnumerator SetObject()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}
