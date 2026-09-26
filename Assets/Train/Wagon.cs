using UnityEngine;

public class Wagon : MonoBehaviour
{
    public int order;

    private int level;

    private int protection;

    private int PV;

    public Train train;

    private void InitPos()
    {
        transform.position = train.FirstPos + new Vector3(0,-order * 2.1f,0);
    }
}
