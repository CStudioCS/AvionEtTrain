using UnityEngine;

public class Usine : Wagon
{
    private float moneyRate = 1.2f;
    private void Update()
    {
        train.money += Time.deltaTime * moneyRate;
        if(isSelected)
        {
            train.money += Time.deltaTime * moneyRate;
        }
    }
}
