using UnityEngine;

public class Avion : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 movementInput;
    private bool dashInput;
    private Quaternion defaultRotation = Quaternion.identity;
    [SerializeField] private float playerSpeed;
    [SerializeField] public Rigidbody2D rb;

    [Header("Dash")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    private Dash dash;


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
        //input
        movementInput = inputActions.Avion.Move.ReadValue<Vector2>();
        dashInput = inputActions.Avion.Dash.WasPressedThisFrame();

        //rotation
        if (movementInput.sqrMagnitude > 0.01f)
        {
            float angleDeg = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            if (-50 <= angleDeg && angleDeg <= 50)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
            }
            else
            {
                transform.rotation = defaultRotation;
            }
        }

        //movement
        if (movementInput.sqrMagnitude > 0.01f)
            rb.linearVelocity = movementInput * playerSpeed;

        //abilities
        dash.Update(Time.deltaTime);
        if (dashInput && movementInput.x >= 0)//dash only towards right side of the screen
            dash.TryActivate();
    }
}
