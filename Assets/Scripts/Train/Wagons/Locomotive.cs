using UnityEngine;

public class Locomotive : Wagon
{
    [SerializeField] private float baseSpeed = 60f;
    [Tooltip("Fraction de la vitesse restante quand la locomotive est presque détruite")]
    [SerializeField, Range(0f, 1f)] private float minSpeedFactor = 0.5f;
    [Tooltip("Vitesse en plus par niveau (0.1 = +10 %)")]
    [SerializeField] private float speedBonusPerLevel = 0.1f;
    [Tooltip("Argent gagné par mètre parcouru quand la locomotive est sélectionnée")]
    [SerializeField] private float moneyPerMeter = 0.04f;

    private void Update()
    {
        if (!CanAct) return;

        // une locomotive endommagée ralentit
        float speed = baseSpeed * LevelBonus(speedBonusPerLevel) * Mathf.Lerp(minSpeedFactor, 1f, Health / MaxHealth);
        train.distance += Time.deltaTime * speed;

        if (isSelected)
        {
            train.money += Time.deltaTime * speed * moneyPerMeter;
        }
    }

    protected override void OnDestroyed()
    {
        GameManager.instance.AvionWin();
    }
}
