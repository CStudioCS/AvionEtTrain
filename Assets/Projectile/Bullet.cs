using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;

    public Vector2 dir;

    void Update()
    {
        transform.position = transform.position + (Vector3)(speed * Time.deltaTime * dir);
        if (transform.position.magnitude > 15)
            Destroy(gameObject);
    }

}
