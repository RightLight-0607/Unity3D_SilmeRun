using UnityEngine;

public class Trampoline : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Player player;
        player = collision.gameObject.GetComponent<Player>();
        if(player != null) 
        {
            player.rb.AddForce(Vector3.up * player.jumpPower * 15);
        }
    }
}
