using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 dir = Vector2.up;

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Avion avion = other.GetComponentInParent<Avion>();
        if (avion == null) return;

        avion.TakeDamage(damage);
        Explosion.Spawn(transform.position, 0.2f, new Color(1f, 0.9f, 0.5f, 1f), 0.12f);
        Destroy(gameObject);
    }
}
