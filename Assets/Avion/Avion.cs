using Unity.VisualScripting;
using UnityEngine;

public class Avion : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 movementInput;
    private bool dashInput;

    private bool shootInput;
    [SerializeField] private float playerSpeedX;   // acc�l�ration horizontale (unit�s/s�)
    [SerializeField] private float playerSpeedY;   // vitesse verticale max (unit�s/s)
    [SerializeField] public Rigidbody2D rb;

    [Header("Dash")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float windSpeed;      // d�c�l�ration due au vent (unit�s/s�)
    private Dash dash;

    [Header("Bullet")]

    [SerializeField] private float shootSpeed;
    [SerializeField] private float shootCooldown;
    [SerializeField] GameObject Bullet;
    private Shoot shoot;


    [Header("Graphics")]
    [SerializeField] private float pitchAmplitude;            // amplitude max de l'inclinaison (degr�s), >= 0
    [SerializeField] private float changePitchSpeed;          // vitesse de changement d'inclinaison (degr�s/s)
    [SerializeField] private float backToDefaultPositionSpeed; // entre 0 et 1 : fraction de l'angle conserv�e par frame � 60 FPS

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        dash = new Dash(dashCooldown, dashSpeed, dashDuration, this);
        shoot = new Shoot(shootCooldown, shootSpeed, this, Bullet);
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
        shootInput = inputActions.Avion.Shoot.IsPressed(); //marche aussi si on reste appuyé sur la touche

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
        shoot.Update(Time.deltaTime);
        if (dashInput && movementInput.x >= 0) // dash only towards right side of the screen
            dash.TryActivate();

        if (shootInput)
            shoot.TryActivate();
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