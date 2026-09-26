using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Train : MonoBehaviour
{
    public Train instance;

    public float speed;

    public float MoneyRate;

    public Vector3 FirstPos;

    public Wagon Wagon;

    private List<Wagon> WagonList = new List<Wagon>();

    public bool spawn;

    public void CreateWagon()
    {
        Wagon wagon = Instantiate(Wagon,transform);
        wagon.train = this;
        wagon.order = WagonList.Count;
        if(WagonList.Count == 0)
        {
            wagon.transform.position = FirstPos;
        }
        else
        {
            wagon.transform.position = WagonList[WagonList.Count - 1].transform.position - Wagon.transform.localScale.x * Vector3.right * 1.1f;
        }

        WagonList.Add(wagon);
    }

    public void Update()
    {
        if(spawn)
        {
            spawn = false;
            CreateWagon();
        }
    }
}
