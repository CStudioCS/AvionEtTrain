using System.Collections;
using UnityEngine;

public class Avion : MonoBehaviour, IDamageable
{
    [Header("Vie")]
    [SerializeField] private float maxHealth = 5f;
    [SerializeField] private float invincibilityDuration = 1f; // après un coup, l'avion ne prend plus de dégâts
    [SerializeField] private float blinkInterval = 0.08f;
    [SerializeField] private float hitFlashDuration = 0.12f;
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.25f, 0.25f, 1f);

    public float Health { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public bool IsInvincible => invincibilityRemaining > 0f;

    private float invincibilityRemaining;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private Coroutine hitRoutine;

    private Mitrailleuse mitrailleuse;
    private LanceBombes lanceBombes;

    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 movementInput;
    private bool dashInput;
    [SerializeField] private float playerSpeedX;   // acc�l�ration horizontale (unit�s/s�)
    [SerializeField] private float playerSpeedY;   // vitesse verticale max (unit�s/s)
    [SerializeField] public Rigidbody2D rb;

    [Header("Dash")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float windSpeed;      // d�c�l�ration due au vent (unit�s/s�)
    private Dash dash;

    [Header("Graphics")]
    [SerializeField] private float pitchAmplitude;            // amplitude max de l'inclinaison (degr�s), >= 0
    [SerializeField] private float changePitchSpeed;          // vitesse de changement d'inclinaison (degr�s/s)
    [SerializeField] private float backToDefaultPositionSpeed; // entre 0 et 1 : fraction de l'angle conserv�e par frame � 60 FPS

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        Health = maxHealth;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].color;

        mitrailleuse = GetComponent<Mitrailleuse>();
        lanceBombes = GetComponent<LanceBombes>();
    }

    private void Start()
    {
        dash = new Dash(dashCooldown, dashSpeed, dashDuration, this);
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        // input (lu dans Update pour ne jamais rater un appui)
        movementInput = inputActions.Avion.Move.ReadValue<Vector2>();
        dashInput = inputActions.Avion.Dash.WasPressedThisFrame();
        bool shootHeld = inputActions.Avion.Shoot.IsPressed();
        bool bombInput = inputActions.Avion.Bomb.WasPressedThisFrame();

        // rotation
        float currentAngle = (transform.rotation.eulerAngles.z + 180f) % 360f - 180f;
        float newAngle;

        if (Mathf.Abs(movementInput.y) > 0.01f)
        {
            newAngle = currentAngle + changePitchSpeed * Mathf.Sign(movementInput.y) * Time.deltaTime;
            newAngle = Mathf.Clamp(newAngle, -pitchAmplitude, pitchAmplitude);
        }
        else
        {
            // m�me comportement qu'avant � 60 FPS, mais ind�pendant du framerate
            float decay = Mathf.Pow(backToDefaultPositionSpeed, Time.deltaTime * 60f);
            newAngle = currentAngle * decay;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

        // abilities
        dash.Update(Time.deltaTime);
        if (dashInput && movementInput.x >= 0) // dash only towards right side of the screen
            dash.TryActivate();

        // armes
        if (mitrailleuse != null) mitrailleuse.SetTrigger(shootHeld);
        if (bombInput && lanceBombes != null) lanceBombes.TryDrop();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || IsInvincible || GameManager.instance.IsGameOver) return;

        Health = Mathf.Max(0f, Health - damage);
        Explosion.Spawn(transform.position, 0.35f, new Color(1f, 0.5f, 0.2f, 0.9f), 0.2f);

        if (Health <= 0f)
        {
            Die();
            return;
        }

        invincibilityRemaining = invincibilityDuration;
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitFeedback());
    }

    private IEnumerator HitFeedback()
    {
        // flash rouge
        SetColor(hitFlashColor, 1f);
        yield return new WaitForSeconds(hitFlashDuration);

        // clignotement pendant l'invincibilité
        bool visible = false;
        while (invincibilityRemaining > 0f)
        {
            SetColor(null, visible ? 1f : 0.3f);
            visible = !visible;
            yield return new WaitForSeconds(blinkInterval);
            invincibilityRemaining -= blinkInterval;
        }

        invincibilityRemaining = 0f;
        SetColor(null, 1f);
        hitRoutine = null;
    }

    // tint == null : couleur d'origine
    private void SetColor(Color? tint, float alpha)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Color c = tint ?? baseColors[i];
            c.a = baseColors[i].a * alpha;
            renderers[i].color = c;
        }
    }

    private void Die()
    {
        IsDead = true;
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        if (mitrailleuse != null) mitrailleuse.SetTrigger(false);
        Explosion.Spawn(transform.position, 1f, new Color(1f, 0.55f, 0.1f, 0.9f), 0.5f);
        SetColor(new Color(0.35f, 0.35f, 0.35f, 1f), 1f);

        // perte de contrôle : l'avion tombe en vrille
        enabled = false; // coupe Update/FixedUpdate et les inputs
        rb.gravityScale = 2f;
        rb.angularVelocity = 360f;

        GameManager.instance.TrainWin();
    }

    void FixedUpdate()
    {
        // movement (pas de temps fixe de la physique)
        float dt = Time.fixedDeltaTime;

        rb.linearVelocityX -= windSpeed * dt;
        if (movementInput.sqrMagnitude > 0.01f)
            rb.linearVelocityX += movementInput.x * playerSpeedX * dt;

        rb.linearVelocityY = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z) * playerSpeedY;

        if(transform.position.x <= -8f)
        {
            transform.position = new Vector3(-8f, transform.position.y, transform.position.z);
            rb.linearVelocityX = Mathf.Max(0f, rb.linearVelocityX);
        }
        if (transform.position.x >= 7f)
        {
            transform.position = new Vector3(7f, transform.position.y, transform.position.z);
            rb.linearVelocityX = Mathf.Min(0f, rb.linearVelocityX);
        }
        if (transform.position.y <= -2f)
        {
            transform.position = new Vector3(transform.position.x, -2f, transform.position.z);
            rb.linearVelocityY = Mathf.Max(0f, rb.linearVelocityY);
        }
        if (transform.position.y >= 4f)
        {
            transform.position = new Vector3(transform.position.x, 4f, transform.position.z);
            rb.linearVelocityY = Mathf.Min(0f, rb.linearVelocityY);
        }

    }
}