using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Train : MonoBehaviour
{
    public Train instance;

    public float distance;
    public float money;

    public Vector3 FirstPos;

    [SerializeField] private List<Wagon> WagonPrefab = new List<Wagon>();
    private List<Wagon> WagonList = new List<Wagon>();

    private int SelectedWagon;

    private void Start()
    {
        instance = this;
        for(int i = 0; i < WagonPrefab.Count; i++)
        {
            InstantiateWagon(i);
        }
        distance = 0;
        money = 0;
    }

    public void InstantiateWagon(int i)
    {
        if(i >= WagonPrefab.Count || i < 0) return;

        Wagon wagon = Instantiate(WagonPrefab[i], transform);

        wagon.train = this;
        wagon.order = WagonList.Count;
        if(i == 0)
        {
            wagon.transform.position = FirstPos;
            wagon.isSelected = true;
            SelectedWagon = 0;
        }
        else
        {
            wagon.transform.position = WagonList[WagonList.Count - 1].transform.position - (WagonList[WagonList.Count - 1].GetComponent<SpriteRenderer>().bounds.size.x + wagon.GetComponent<SpriteRenderer>().bounds.size.x) * Vector3.right * 0.5f * 1.1f;
            wagon.isSelected = false;
        }

        WagonList.Add(wagon);
    }

    public void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame) MoveSelection(+1);
        if (kb.rightArrowKey.wasPressedThisFrame) MoveSelection(-1);
    }
    public void MoveSelection(int delta)
    {
        if (WagonList.Count == 0) return;

        WagonList[SelectedWagon].isSelected = false;
        int next = (SelectedWagon + delta + WagonList.Count) % WagonList.Count;
        SelectedWagon = next;
        WagonList[SelectedWagon].isSelected = true;

    }
}
