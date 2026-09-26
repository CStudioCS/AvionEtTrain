using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;

    private Vector2 dir;

    void Update()
    {
        transform.position = transform.position + (Vector3)(dir * speed);
    }
}
