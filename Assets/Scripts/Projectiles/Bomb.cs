using System.Collections.Generic;
using UnityEngine;

// Bombe larguée par l'avion : tombe avec la gravité et explose au contact d'un wagon
// (ou au sol), en abîmant tous les wagons dans le rayon.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    [SerializeField] private float damage = 6f;
    [SerializeField] private float explosionRadius = 1.1f;
    [SerializeField] private float groundY = -4.8f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;
    private bool exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 initialVelocity)
    {
        rb.linearVelocity = initialVelocity;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // la bombe pique dans le sens de sa chute
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude > 0.01f)
            rb.rotation = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        if (transform.position.y <= groundY) Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Wagon wagon = other.GetComponentInParent<Wagon>();
        if (wagon != null && !wagon.IsDead) Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        var hit = new HashSet<Wagon>();
        foreach (Collider2D col in Physics2D.OverlapCircleAll(transform.position, explosionRadius))
        {
            Wagon wagon = col.GetComponentInParent<Wagon>();
            if (wagon != null && hit.Add(wagon)) wagon.TakeDamage(damage);
        }

        Explosion.Spawn(transform.position, explosionRadius, new Color(1f, 0.6f, 0.15f, 0.9f), 0.35f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
