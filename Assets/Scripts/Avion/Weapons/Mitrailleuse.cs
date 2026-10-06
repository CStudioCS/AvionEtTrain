using UnityEngine;

// Mitrailleuse de l'avion : tire dans l'axe du nez tant que la touche est maintenue.
// Elle chauffe à chaque tir ; en surchauffe elle est bloquée jusqu'à refroidissement complet.
[RequireComponent(typeof(Avion))]
public class Mitrailleuse : MonoBehaviour
{
    [SerializeField] private AvionBullet bulletPrefab;
    [SerializeField] private float fireRate = 9f;                          // tirs par seconde
    [SerializeField] private float spread = 3f;                            // dispersion (degrés)
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0.5f, 0f); // repère local de l'avion

    [Header("Surchauffe")]
    [SerializeField] private float heatPerShot = 0.08f;        // 1 = surchauffe, soit ~12 balles d'affilée
    [SerializeField] private float coolingDelay = 0.25f;       // pause avant de refroidir
    [SerializeField] private float coolingRate = 0.6f;         // par seconde
    [SerializeField] private float overheatCoolingRate = 0.45f; // refroidissement (plus lent) en surchauffe

    [Header("Jauge")]
    [SerializeField] private Vector2 gaugeOffset = new Vector2(0f, 0.7f);
    [SerializeField] private Vector2 gaugeSize = new Vector2(1f, 0.06f);
    [SerializeField] private Color coldColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private Color hotColor = new Color(1f, 0.3f, 0.1f);

    public float Heat { get; private set; }
    public bool Overheated { get; private set; }

    private Avion avion;
    private bool triggerHeld;
    private float shotCooldown;
    private float sinceLastShot;
    private WorldBar gauge;

    private void Awake()
    {
        avion = GetComponent<Avion>();
    }

    private void Start()
    {
        gauge = new WorldBar("Mitrailleuse_Heat", gaugeSize, 0.02f, 50, new Color(0f, 0f, 0f, 0.5f));
    }

    public void SetTrigger(bool held)
    {
        triggerHeld = held;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        shotCooldown -= dt;
        sinceLastShot += dt;

        bool canFire = triggerHeld && !Overheated && !avion.IsDead && !GameManager.instance.IsGameOver;
        if (canFire && shotCooldown <= 0f)
        {
            Fire();
            shotCooldown = 1f / fireRate;
            sinceLastShot = 0f;

            Heat += heatPerShot;
            if (Heat >= 1f)
            {
                Heat = 1f;
                Overheated = true;
            }
        }
        else if (Overheated)
        {
            Heat -= overheatCoolingRate * dt;
            if (Heat <= 0f)
            {
                Heat = 0f;
                Overheated = false;
            }
        }
        else if (sinceLastShot >= coolingDelay)
        {
            Heat = Mathf.Max(0f, Heat - coolingRate * dt);
        }
    }

    private void Fire()
    {
        if (bulletPrefab == null) return;

        Vector3 pos = transform.TransformPoint(muzzleOffset);
        float angle = (transform.eulerAngles.z + Random.Range(-spread, spread)) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        AvionBullet bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bullet.Init(dir);
    }

    private void LateUpdate()
    {
        if (gauge == null) return;

        gauge.SetVisible(Heat > 0f && !avion.IsDead);
        gauge.SetPosition(transform.position + (Vector3)gaugeOffset);

        // en surchauffe la jauge clignote
        Color c = Color.Lerp(coldColor, hotColor, Heat);
        if (Overheated && Mathf.Repeat(Time.time, 0.2f) < 0.1f) c = Color.white;
        gauge.SetFill(Heat, c);
    }

    private void OnDestroy()
    {
        gauge?.Destroy();
    }
}
