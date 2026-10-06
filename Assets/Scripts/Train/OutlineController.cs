using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OutlineController : MonoBehaviour
{
    [SerializeField] private Color color = new Color(1f, 0.8f, 0.3f, 1f);
    [SerializeField] private float radius = 6f;
    [SerializeField] private Wagon wagon;

    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;
    private bool outlineState;

    private static readonly int ColorId = Shader.PropertyToID("_GlowColor");
    private static readonly int RadiusId = Shader.PropertyToID("_GlowRadius");
    private static readonly int IntensityId = Shader.PropertyToID("_GlowIntensity");
    [SerializeField] private float intensity = 1.5f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        outlineState = false;
        SetOutline(false);
    }

    public void SetOutline(bool on)
    {
        sr.GetPropertyBlock(mpb);
        mpb.SetColor(ColorId, color);
        mpb.SetFloat(RadiusId, radius);
        mpb.SetFloat(IntensityId, on ? intensity : 0f);
        sr.SetPropertyBlock(mpb);
    }

    private void Update()
    {
        if (wagon.isSelected != outlineState)
        {
            outlineState = wagon.isSelected;
            SetOutline(outlineState);
        }
    }
}