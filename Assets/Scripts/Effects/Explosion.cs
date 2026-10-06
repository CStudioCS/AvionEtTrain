using UnityEngine;

// Effet d'explosion simple : un disque qui grossit et s'efface.
// Aucun prefab nécessaire, le sprite rond est généré une fois au runtime.
public class Explosion : MonoBehaviour
{
    private static Sprite circle;

    private SpriteRenderer sr;
    private Color color;
    private float diameter;
    private float duration;
    private float timer;

    public static void Spawn(Vector3 position, float radius, Color color, float duration = 0.3f)
    {
        var go = new GameObject("Explosion");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Circle();
        sr.color = color;
        sr.sortingOrder = 40;

        var e = go.AddComponent<Explosion>();
        e.sr = sr;
        e.color = color;
        e.diameter = radius * 2f;
        e.duration = duration;
        e.Apply(0f);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float k = timer / duration;
        if (k >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        Apply(k);
    }

    private void Apply(float k)
    {
        float ease = 1f - (1f - k) * (1f - k);
        float s = Mathf.Lerp(0.3f, 1f, ease) * diameter;
        transform.localScale = new Vector3(s, s, 1f);

        Color c = color;
        c.a *= 1f - k;
        sr.color = c;
    }

    // disque blanc à bord doux, une unité de diamètre
    private static Sprite Circle()
    {
        if (circle != null) return circle;

        const int res = 64;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float r = res * 0.5f;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
            float a = Mathf.Clamp01((1f - d) * 6f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();

        circle = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
        return circle;
    }
}
