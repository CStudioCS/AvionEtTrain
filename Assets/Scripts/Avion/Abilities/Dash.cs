using UnityEngine;

public class Dash : Ability
{
    private float dashSpeed;
    private float dashDuration;
    private float dashRemainingDuration;
    private Avion avion;

    public Dash(float cooldown, float dashSpeed, float dashDuration, Avion avion) : base(cooldown)
    {
        this.dashSpeed = dashSpeed;
        this.avion = avion;
        this.dashDuration = dashDuration;
    }

    protected override void UseAbility()
    {
        dashRemainingDuration = dashDuration;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (dashRemainingDuration > 0)
        {
            dashRemainingDuration -= deltaTime;
            Vector2 direction = avion.movementInput;
            avion.rb.linearVelocity += direction * dashSpeed;
        }

    }
}
