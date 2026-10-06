using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Missile : MonoBehaviour
{
    [Header("Lancement")]
    [SerializeField] private float launchSpeed = 3f;      // vitesse de montée au-dessus du wagon
    [SerializeField] private float launchDuration = 0.6f; // durée avant activation

    [Header("Activé")]
    [SerializeField] private float speed = 11f;
    [SerializeField] private float damage = 2f;
    [SerializeField] private float lifeTime = 8f;

    private Avion target;
    private Vector2 dir = Vector2.up;
    private float timer;
    private bool active;

    public void Init(Avion avion)
    {
        target = avion;
    }

    private void Start()
    {
        SetRotation();
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (!active)
        {
            // phase de lancement : le missile monte en ralentissant
            timer += dt;
            float t = Mathf.Clamp01(timer / launchDuration);
            transform.position += (Vector3)(dir * launchSpeed * (1f - t) * dt);
            if (timer >= launchDuration) Activate();
            return;
        }

        // phase active : ligne droite vers la position de l'avion au moment de l'activation
        transform.position += (Vector3)(dir * speed * dt);
    }

    private void Activate()
    {
        active = true;
        if (target != null)
        {
            dir = ((Vector2)(target.transform.position - transform.position)).normalized;
            SetRotation();
        }
    }

    private void SetRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        Avion avion = other.GetComponentInParent<Avion>();
        if (avion == null) return;

        avion.TakeDamage(damage);
        Explosion.Spawn(transform.position, 0.6f, new Color(1f, 0.45f, 0.1f, 0.9f), 0.3f);
        Destroy(gameObject);
    }

    // abattu par la mitrailleuse de l'avion
    public void Intercept()
    {
        Explosion.Spawn(transform.position, 0.5f, new Color(1f, 0.8f, 0.3f, 0.9f), 0.25f);
        Destroy(gameObject);
    }
}
