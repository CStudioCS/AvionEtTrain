using UnityEngine;

public class LanceMissile : Wagon
{
    [SerializeField] private Missile missilePrefab;
    [SerializeField] private float fireInterval = 4f;     // secondes entre deux missiles
    [SerializeField] private Vector2 launchOffset = new Vector2(0f, 0.7f);
    [Tooltip("Cadence de tir en plus par niveau (0.3 = +30 %)")]
    [SerializeField] private float fireRateBonusPerLevel = 0.3f;

    private Avion target;
    private float cooldown;

    private void Update()
    {
        if (!CanAct) return;

        // le rechargement continue même si le wagon n'est pas sélectionné
        if (cooldown > 0f) cooldown -= Time.deltaTime;

        if (!isSelected || cooldown > 0f) return;

        if (target == null)
        {
            target = FindFirstObjectByType<Avion>();
            if (target == null) return;
        }
        if (target.IsDead) return;

        Shoot();
        cooldown = fireInterval / LevelBonus(fireRateBonusPerLevel);
    }

    private void Shoot()
    {
        if (missilePrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3)launchOffset;
        Missile missile = Instantiate(missilePrefab, spawnPos, Quaternion.identity);
        missile.Init(target);
    }
}
