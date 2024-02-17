using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LodingEffect : MonoBehaviour
{
    Image image;
    float r, g, b;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        r = image.color.r;
        g = image.color.g;
        b = image.color.b;
    }
    private void OnEnable()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float a = 1;
        yield return new WaitForSeconds(1);
        while (a > 0)
        {
            a -= 0.01f;
            image.color = new Color(r, g, b, a);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
