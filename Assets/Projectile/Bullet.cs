using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;

    public Vector2 dir;

    void Update()
    {
        transform.position = transform.position + (Vector3)(speed * Time.deltaTime * dir);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Debug.Log(screenPos);
        if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height)
            Destroy(gameObject);
    }

}
