using UnityEngine;

public class EnemyChase3D : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 3f;

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            
            // Keep using Update() for non-physics movement
            // If you add Rigidbody to enemies later, move this to FixedUpdate()
            transform.position += direction * chaseSpeed * Time.deltaTime;
            transform.LookAt(player);
        }
    }
}
