using UnityEngine;

public class Usine : Wagon
{
    [SerializeField] private float moneyRate = 3f;              // $/s, doublé quand l'usine est sélectionnée
    [Tooltip("Revenu en plus par niveau (0.5 = +50 %)")]
    [SerializeField] private float incomeBonusPerLevel = 0.5f;

    private void Update()
    {
        if (!CanAct) return;

        float rate = moneyRate * LevelBonus(incomeBonusPerLevel);
        if (isSelected) rate *= 2f;
        train.money += Time.deltaTime * rate;
    }
}
