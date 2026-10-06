using UnityEngine;

// Barre de vie au-dessus de n'importe quel IDamageable (avion, wagons).
// Elle n'est pas enfant de l'objet pour ne pas suivre sa rotation ni son échelle.
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.1f);
    [SerializeField] private Vector2 size = new Vector2(1.4f, 0.16f);
    [SerializeField] private float border = 0.04f;
    [SerializeField] private int sortingOrder = 50;

    [Header("Couleurs")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color trailColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color fullColor = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color lowColor = new Color(0.95f, 0.2f, 0.2f);

    [Header("Traînée de dégâts")]
    [SerializeField] private float trailDelay = 0.35f;  // temps avant que la traînée descende
    [SerializeField] private float trailSpeed = 1.5f;   // fraction de barre par seconde

    [Header("Niveau (si l'objet est améliorable)")]
    [SerializeField] private float pipSize = 0.12f;
    [SerializeField] private float pipSpacing = 0.06f;
    [SerializeField] private Color pipOnColor = new Color(1f, 0.85f, 0.25f);
    [SerializeField] private Color pipOffColor = new Color(0.25f, 0.25f, 0.25f);

    private IDamageable target;
    private IUpgradable upgradable;
    private WorldBar bar;
    private WorldBar[] pips;

    private float trailRatio = 1f;
    private float lastRatio = 1f;
    private float trailTimer;

    private void Awake()
    {
        target = GetComponent<IDamageable>();
        upgradable = GetComponent<IUpgradable>();
        if (target == null)
            Debug.LogError("HealthBar : aucun IDamageable sur cet objet.", this);
    }

    private void Start()
    {
        bar = new WorldBar(name + "_HealthBar", size, border, sortingOrder, backgroundColor, trailColor);

        if (upgradable != null && upgradable.MaxLevel > 1)
        {
            pips = new WorldBar[upgradable.MaxLevel];
            for (int i = 0; i < pips.Length; i++)
                pips[i] = new WorldBar(name + "_LevelPip", new Vector2(pipSize, pipSize), 0.02f, sortingOrder, backgroundColor);
        }
    }

    private void LateUpdate()
    {
        if (bar == null || target == null) return;

        bar.SetPosition(transform.position + (Vector3)offset);

        UpdatePips();

        if (target.IsDead)
        {
            bar.SetVisible(false);
            return;
        }

        float ratio = target.MaxHealth > 0f ? target.Health / target.MaxHealth : 0f;

        if (ratio < lastRatio) trailTimer = trailDelay;
        lastRatio = ratio;

        if (trailTimer > 0f) trailTimer -= Time.deltaTime;
        else trailRatio = Mathf.MoveTowards(trailRatio, ratio, trailSpeed * Time.deltaTime);
        if (trailRatio < ratio) trailRatio = ratio;

        bar.SetFill(ratio, Color.Lerp(lowColor, fullColor, ratio));
        bar.SetTrail(trailRatio);
    }

    // pastilles de niveau, centrées juste sous la barre
    private void UpdatePips()
    {
        if (pips == null) return;

        float step = pipSize + pipSpacing;
        float startX = -step * (pips.Length - 1) * 0.5f;
        Vector3 basePos = transform.position + (Vector3)offset + Vector3.down * (size.y * 0.5f + border + pipSize * 0.5f + 0.06f);

        for (int i = 0; i < pips.Length; i++)
        {
            pips[i].SetVisible(!target.IsDead);
            pips[i].SetPosition(basePos + Vector3.right * (startX + i * step));
            pips[i].SetFill(1f, i < upgradable.Level ? pipOnColor : pipOffColor);
        }
    }

    private void OnDestroy()
    {
        bar?.Destroy();
        if (pips != null)
            foreach (var pip in pips) pip.Destroy();
    }
}
