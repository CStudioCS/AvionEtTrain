using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public Vector2 dir;
    public Vector2 defaultDir = new Vector2(1, 0);

    void Update()
    {
        Debug.Log(speed);
        Debug.Log(dir);
        if(dir == Vector2.zero)
            transform.position = transform.position + (Vector3)(defaultDir * speed* Time.deltaTime);
        else
            transform.position = transform.position + (Vector3)(dir * speed* Time.deltaTime);
 
    }
}
