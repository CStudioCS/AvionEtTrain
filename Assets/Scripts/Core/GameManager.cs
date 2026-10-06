using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool IsGameOver { get; private set; }

    [SerializeField] private int targetDistance;
    [SerializeField] private TMP_Text endOfGameText;
    [SerializeField] private TMP_Text distanceText;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!IsGameOver && Train.instance.distance >= targetDistance)
        {
            TrainWin();
        }
        UpdateDistanceText(Train.instance.distance);
    }

    public void TrainWin()
    {
        EndGame("Train Win!");
    }

    public void AvionWin()
    {
        EndGame("Avion Win!");
    }

    private void EndGame(string message)
    {
        if (IsGameOver) return;
        IsGameOver = true;

        endOfGameText.gameObject.SetActive(true);
        endOfGameText.text = message;
    }

    private void UpdateDistanceText(float distance)
    {
        distanceText.text = "Distance: " + distance.ToString("F0") + " m";
    }
}
