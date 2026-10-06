using UnityEngine;

// Largue une bombe sous l'avion. Elle garde une partie de l'élan de l'avion,
// il faut donc anticiper le largage.
[RequireComponent(typeof(Avion))]
public class LanceBombes : MonoBehaviour
{
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private float reloadTime = 2.5f;
    [SerializeField] private Vector2 dropOffset = new Vector2(0f, -0.3f);  // repère monde
    [SerializeField, Range(0f, 1f)] private float inheritVelocity = 0.6f;

    [Header("Jauge de rechargement")]
    [SerializeField] private Vector2 gaugeOffset = new Vector2(0f, 0.6f);
    [SerializeField] private Vector2 gaugeSize = new Vector2(1f, 0.05f);
    [SerializeField] private Color gaugeColor = new Color(0.4f, 0.7f, 1f);

    public bool IsReady => reload <= 0f;

    private Avion avion;
    private float reload;
    private WorldBar gauge;

    private void Awake()
    {
        avion = GetComponent<Avion>();
    }

    private void Start()
    {
        gauge = new WorldBar("LanceBombes_Reload", gaugeSize, 0.02f, 50, new Color(0f, 0f, 0f, 0.5f));
    }

    public bool TryDrop()
    {
        if (!IsReady || avion.IsDead || GameManager.instance.IsGameOver || bombPrefab == null)
            return false;

        Vector3 pos = transform.position + (Vector3)dropOffset;
        Bomb bomb = Instantiate(bombPrefab, pos, Quaternion.identity);
        bomb.Init(avion.rb.linearVelocity * inheritVelocity);

        reload = reloadTime;
        return true;
    }

    private void Update()
    {
        if (reload > 0f) reload -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (gauge == null) return;

        // visible seulement pendant le rechargement
        gauge.SetVisible(!IsReady && !avion.IsDead);
        gauge.SetPosition(transform.position + (Vector3)gaugeOffset);
        gauge.SetFill(1f - reload / reloadTime, gaugeColor);
    }

    private void OnDestroy()
    {
        gauge?.Destroy();
    }
}
