// Tout ce qui a des points de vie et peut être touché (avion, wagons).
public interface IDamageable
{
    float Health { get; }
    float MaxHealth { get; }
    bool IsDead { get; }

    void TakeDamage(float damage);
}
