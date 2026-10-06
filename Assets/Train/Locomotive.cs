using UnityEngine;

public class Locomotive : Wagon
{
    [SerializeField] private float baseSpeed = 60f;
    private void Update()
    {
        train.distance += Time.deltaTime * baseSpeed;

        if (isSelected)
        {
            train.distance += Time.deltaTime * baseSpeed * train.speedMultiplierWhenSelected; ;
        }
    }

}
