using UnityEngine;

// Petite barre dans le monde (fond + remplissage + traînée optionnelle), construite en sprites.
// Elle n'est enfant de rien : c'est au propriétaire de la placer avec SetPosition.
public class WorldBar
{
    private static Sprite pixel;

    private readonly Transform root;
    private readonly SpriteRenderer fill;
    private readonly SpriteRenderer trail;
    private readonly Vector2 size;

    public WorldBar(string name, Vector2 size, float border, int sortingOrder, Color backgroundColor, Color? trailColor = null)
    {
        this.size = size;
        root = new GameObject(name).transform;

        SpriteRenderer bg = CreatePart("Background", backgroundColor, sortingOrder);
        bg.transform.localPosition = new Vector3(-size.x * 0.5f - border, 0f, 0f);
        bg.transform.localScale = new Vector3(size.x + border * 2f, size.y + border * 2f, 1f);

        if (trailColor.HasValue)
        {
            trail = CreatePart("Trail", trailColor.Value, sortingOrder + 1);
            trail.transform.localPosition = new Vector3(-size.x * 0.5f, 0f, 0f);
        }

        fill = CreatePart("Fill", Color.white, sortingOrder + 2);
        fill.transform.localPosition = new Vector3(-size.x * 0.5f, 0f, 0f);

        SetFill(1f, Color.white);
        SetTrail(1f);
    }

    private SpriteRenderer CreatePart(string name, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Pixel();
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    public void SetPosition(Vector3 position) => root.position = position;

    public void SetVisible(bool visible)
    {
        if (root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
    }

    public void SetFill(float ratio, Color color)
    {
        fill.transform.localScale = new Vector3(size.x * Mathf.Clamp01(ratio), size.y, 1f);
        fill.color = color;
    }

    public void SetTrail(float ratio)
    {
        if (trail == null) return;
        trail.transform.localScale = new Vector3(size.x * Mathf.Clamp01(ratio), size.y, 1f);
    }

    public void Destroy()
    {
        if (root != null) Object.Destroy(root.gameObject);
    }

    // sprite blanc d'une unité, pivot à gauche : l'échelle X donne directement la longueur
    private static Sprite Pixel()
    {
        if (pixel == null)
            pixel = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
        return pixel;
    }
}
