using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // 방향 표시로 활용할 화살표의 점멸효과
    SpriteRenderer sr;
    Color alpha = new Color(0, 0, 0, 0.1f);
    bool isUpAlpha = false;
    [SerializeField] float ratingTime;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(ratingTime);
        while (true)
        {
            sr.color += isUpAlpha ? alpha * 0.1f : alpha * -0.1f;

            if (sr.color.a >= 1)
                isUpAlpha = false;
            else if (sr.color.a <= 0)
                isUpAlpha = true;

            yield return null;
        }
    }
}
