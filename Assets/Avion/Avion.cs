using System;
using System.Linq.Expressions;
using UnityEngine;

public class Avion : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    [HideInInspector] public Vector2 movementInput;
    private bool dashInput;
    private Quaternion defaultRotation = Quaternion.identity;
    [SerializeField] private float playerSpeedX;
    [SerializeField] private float playerSpeedY;
    [SerializeField] public Rigidbody2D rb;

    [Header("Dash")]
    [SerializeField] private float dashCooldown;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float windSpeed;
    private Dash dash;

    [Header("Graphics")]
    [SerializeField] private float pitchAmplitude;//l'amplitude de l'orientation verticale de l'avion, >=0.
    [SerializeField] private float changePitchSpeed;//la vitesse à laquelle l'avion change d'orientation
    [SerializeField] private float backToDefaultPositionSpeed;//Entre 0 et 1.


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
        if (Mathf.Abs(movementInput.y) > 0.01f)
        {
            float angleDeg = 90 - Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg ;
            float newAngle = (transform.rotation.eulerAngles.z+180)%360 - 180 + changePitchSpeed * Mathf.Sign(movementInput.y);
            if (Mathf.Abs(newAngle)<=pitchAmplitude)
            {
                transform.rotation = Quaternion.Euler(0f, 0f,newAngle);
            }
        }
        else
        {
 
            transform.rotation = Quaternion.Euler(0f, 0f,((transform.rotation.eulerAngles.z + 180f) % 360f - 180f) * backToDefaultPositionSpeed);
        }
        
        
        //movement
        rb.linearVelocityX -= windSpeed;
        if (movementInput.sqrMagnitude > 0.01f)
            rb.linearVelocityX += movementInput.x * playerSpeedX;
            rb.linearVelocityY = Mathf.Sin(Mathf.Deg2Rad*transform.rotation.eulerAngles.z) * playerSpeedY;




        //abilities
        dash.Update(Time.deltaTime);
        if (dashInput && movementInput.x >= 0)//dash only towards right side of the screen
            dash.TryActivate();
    }
}
