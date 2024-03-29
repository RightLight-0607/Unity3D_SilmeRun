using System.Collections;
using TMPro;
using UnityEngine;

public class Story : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI story;
    [SerializeField] TextMeshProUGUI pushSpace;
    [SerializeField] bool isUpAlpha = false;
    Color alpha = new Color(0, 0, 0, 0.1f);
    Coroutine storyCoru;
    void Start()
    {
        storyCoru = StartCoroutine(StoryScroll());
    }

    private void Update()
    {
        // a값이 0이되면 상승, 1이되면 하락

        pushSpace.color += isUpAlpha ? alpha * 0.05f : alpha * -0.05f;
        
        if (pushSpace.color.a >= 1)
            isUpAlpha = false;
        else if (pushSpace.color.a <= 0)
            isUpAlpha = true;
        
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            Destroy(gameObject);
        }
    }

    IEnumerator StoryScroll()
    {
        while (story.transform.position.y <= 600) 
        {
            story.transform.position += transform.up * Time.deltaTime * 100;
            yield return null;
        }
    }
}
