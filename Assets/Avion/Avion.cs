using UnityEngine;

public class Avion : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 movementInput;
    private bool dashInput;
    [SerializeField] private float playerSpeedX;   // accélération horizontale (unités/s²)
    [SerializeField] private float playerSpeedY;   // vitesse verticale max (unités/s)
    [SerializeField] public Rigidbody2D rb;

    [Header("Dash")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float windSpeed;      // décélération due au vent (unités/s²)
    private Dash dash;

    [Header("Graphics")]
    [SerializeField] private float pitchAmplitude;            // amplitude max de l'inclinaison (degrés), >= 0
    [SerializeField] private float changePitchSpeed;          // vitesse de changement d'inclinaison (degrés/s)
    [SerializeField] private float backToDefaultPositionSpeed; // entre 0 et 1 : fraction de l'angle conservée par frame à 60 FPS

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
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
            // même comportement qu'avant à 60 FPS, mais indépendant du framerate
            float decay = Mathf.Pow(backToDefaultPositionSpeed, Time.deltaTime * 60f);
            newAngle = currentAngle * decay;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);

        // abilities
        dash.Update(Time.deltaTime);
        if (dashInput && movementInput.x >= 0) // dash only towards right side of the screen
            dash.TryActivate();
    }

    void FixedUpdate()
    {
        // movement (pas de temps fixe de la physique)
        float dt = Time.fixedDeltaTime;

        rb.linearVelocityX -= windSpeed * dt;
        if (movementInput.sqrMagnitude > 0.01f)
            rb.linearVelocityX += movementInput.x * playerSpeedX * dt;

        rb.linearVelocityY = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z) * playerSpeedY;
    }
}