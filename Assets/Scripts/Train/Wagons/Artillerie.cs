using UnityEngine;

public class Artillerie : Wagon
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private float fireRate = 0.5f;          // tirs par seconde
    [SerializeField] private float selectedFireRateMultiplier = 3f;
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0f, 0.6f);
    [Tooltip("Cadence de tir en plus par niveau (0.35 = +35 %)")]
    [SerializeField] private float fireRateBonusPerLevel = 0.35f;

    private Avion target;
    private float cooldown;

    private void Update()
    {
        if (!CanAct) return;

        if (target == null)
        {
            target = FindFirstObjectByType<Avion>();
            if (target == null) return;
        }
        if (target.IsDead) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        Shoot();

        float rate = fireRate * LevelBonus(fireRateBonusPerLevel);
        if (isSelected) rate *= selectedFireRateMultiplier;
        cooldown = 1f / rate;
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3)muzzleOffset;
        Vector2 dir = target.transform.position - spawnPos;

        Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.Init(dir);
    }
}
