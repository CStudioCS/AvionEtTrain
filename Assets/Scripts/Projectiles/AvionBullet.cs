using UnityEngine;

// Balle de mitrailleuse de l'avion : abîme les wagons et peut abattre les missiles.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AvionBullet : MonoBehaviour
{
    [SerializeField] private float speed = 16f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifeTime = 1.2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        rb.linearVelocity = dir * speed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // visible dès la première frame
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Wagon wagon = other.GetComponentInParent<Wagon>();
        if (wagon != null)
        {
            if (wagon.IsDead) return; // les balles traversent les épaves
            wagon.TakeDamage(damage);
            Explosion.Spawn(transform.position, 0.15f, new Color(1f, 0.9f, 0.5f, 1f), 0.12f);
            Destroy(gameObject);
            return;
        }

        Missile missile = other.GetComponentInParent<Missile>();
        if (missile != null)
        {
            missile.Intercept();
            Destroy(gameObject);
        }
    }
}
