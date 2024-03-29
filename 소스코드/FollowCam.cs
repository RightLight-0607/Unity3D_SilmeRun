using UnityEngine;

public class FollowCam : MonoBehaviour
{
    GameObject player;
    Vector3 targetPos = new Vector3(0, 3, -5);
    PlayManager manager;
    private void Start()
    {;
        manager = GameObject.Find("PlayManager").GetComponent<PlayManager>();
        player = manager.my;
        this.transform.SetParent(player.transform);
        transform.localPosition = targetPos;
    }
    private void Update()
    {
    }
}
