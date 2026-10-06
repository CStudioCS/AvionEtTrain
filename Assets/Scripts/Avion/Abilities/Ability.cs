public abstract class Ability
{
    private float cooldown;
    private float cooldownRemaining;

    public Ability(float cooldown)
    {
        this.cooldown = cooldown;
    }

    public virtual void Update(float deltaTime)
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= deltaTime;
    }

    public bool TryActivate()
    {
        if (cooldownRemaining > 0)
            return false;

        UseAbility();
        cooldownRemaining = cooldown;
        return true;
    }

    protected abstract void UseAbility();
}