using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int targetDistance;
    [SerializeField] private TMP_Text endOfGameText;
    [SerializeField] private TMP_Text distanceText;

    private void Update()
    {
        if (Train.instance.distance >= targetDistance)
        {
            TrainWin();
        }
        UpdateDistanceText(Train.instance.distance);
    }

    private void TrainWin()
    {
        endOfGameText.gameObject.SetActive(true);
        endOfGameText.text = "Train Win!";
    }

    private void AvionWin()
    {
        endOfGameText.gameObject.SetActive(true);
        endOfGameText.text = "Avion Win!";
    }

    private void UpdateDistanceText(float distance)
    {
        distanceText.text = "Distance: " + distance.ToString("F0") + " m";
    }
}
