using System.Collections;
using UnityEngine;

public class Wagon : MonoBehaviour, IDamageable, IUpgradable
{
    public int order;
    private int protection;

    public bool isSelected;

    public Train train;

    [Header("Vie")]
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color wreckColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Amélioration")]
    [Tooltip("PV max en plus par niveau au-dessus de 1 (0.25 = +25 %)")]
    [SerializeField] private float healthBonusPerLevel = 0.25f;
    [SerializeField] private string displayName = "";

    public float Health { get; private set; }
    public float MaxHealth => maxHealth * LevelBonus(healthBonusPerLevel);
    public bool IsDead { get; private set; }

    public int Level { get; private set; } = 1;
    public int MaxLevel => train != null ? train.MaxLevel : 1;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? GetType().Name : displayName;

    // le wagon ne fait plus rien s'il est détruit ou si la partie est finie
    public bool CanAct => !IsDead && (GameManager.instance == null || !GameManager.instance.IsGameOver);

    // multiplicateur d'un bonus par niveau : 1 au niveau 1, 1 + bonus au niveau 2, etc.
    protected float LevelBonus(float bonusPerLevel) => 1f + bonusPerLevel * (Level - 1);

    private SpriteRenderer sr;
    private Color baseColor;
    private Coroutine flashRoutine;

    protected virtual void Awake()
    {
        Health = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        EnsureHitbox();
    }

    // hitbox en trigger, calée sur le sprite, si le prefab n'en a pas
    private void EnsureHitbox()
    {
        if (GetComponent<Collider2D>() != null) return;

        var box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (sr.sprite != null)
        {
            box.size = sr.sprite.bounds.size;
            box.offset = sr.sprite.bounds.center;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!CanAct) return;

        Health = Mathf.Max(0f, Health - damage);
        if (Health <= 0f)
        {
            Die();
            return;
        }

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(HitFlash());
    }

    public void Repair(float amount)
    {
        if (!CanAct) return;
        Health = Mathf.Min(MaxHealth, Health + amount);
    }

    public void Upgrade()
    {
        if (!CanAct || Level >= MaxLevel) return;

        // les PV gagnés par le niveau sont ajoutés tout de suite
        float before = MaxHealth;
        Level++;
        Health += MaxHealth - before;

        Explosion.Spawn(transform.position, 1.4f, new Color(1f, 0.85f, 0.25f, 0.7f), 0.45f);
        OnUpgraded();
    }

    // appelé après chaque montée de niveau
    protected virtual void OnUpgraded() { }

    private IEnumerator HitFlash()
    {
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        sr.color = baseColor;
        flashRoutine = null;
    }

    private void Die()
    {
        IsDead = true;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        sr.color = wreckColor;

        Explosion.Spawn(transform.position, 1.6f, new Color(1f, 0.55f, 0.1f, 0.9f), 0.5f);

        if (train != null) train.OnWagonDestroyed(this);
        OnDestroyed();
    }

    // appelé une fois quand le wagon est détruit
    protected virtual void OnDestroyed() { }
}
