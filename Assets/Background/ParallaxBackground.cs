using System;
using System.Collections.Generic;
using UnityEngine;

// Fond en parallaxe composé :
//  - de couches de sprites (PNG) recyclés, dont la vitesse, l'échelle, la teinte et l'ordre de rendu
//    découlent d'une unique valeur de distance ;
//  - de fonds tileables, défilant par décalage d'UV sur des Quads enfants de cet objet.
//
// Modèle : facteur apparent f(d) = focal / (focal + d), soit une projection perspective.
// d = 0 -> f = 1 : l'élément se comporte comme le plan de jeu.
// d -> infini -> f -> 0 : l'élément est fixe par rapport à la caméra.
//
// Hypothèse : caméra orthographique, défilement horizontal.
public class ParallaxBackground : MonoBehaviour
{
    [Serializable]
    public class Layer
    {
        public string name = "Couche";
        public Sprite[] sprites;
        [Min(0f)] public float distance = 10f;
        [Min(1), Tooltip("Nombre d'éléments recyclés : doit suffire à couvrir la largeur de vue")]
        public int count = 6;
        [Tooltip("Plage verticale en coordonnées viewport (0 = bas, 1 = haut)")]
        public Vector2 heightRange = new Vector2(0.3f, 0.7f);
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        [Tooltip("Écart horizontal entre deux éléments, en unités monde à distance nulle")]
        public Vector2 gapRange = new Vector2(1f, 4f);
        public bool randomFlipX = true;
    }

    [Serializable]
    public class UVLayer
    {
        [Tooltip("Quad enfant de cet objet, texture en Wrap Mode = Repeat")]
        public Renderer renderer;
        [Min(0f)] public float distance = 300f;
    }

    [Header("Caméra")]
    [SerializeField] private Camera cam;
    [Tooltip("L'objet suit la caméra ; ses déplacements alimentent la parallaxe")]
    [SerializeField] private bool followCamera = true;

    [Header("Défilement")]
    [Tooltip("Vitesse automatique en unités monde/s à distance nulle (négative = vers la droite)")]
    [SerializeField] private float baseSpeed = 3f;
    [Tooltip("Distance à laquelle la vitesse apparente est divisée par deux")]
    [SerializeField, Min(0.01f)] private float focalDistance = 10f;
    [SerializeField] private bool unscaledTime = false;

    [Header("Rendu de la profondeur")]
    [SerializeField] private bool scaleWithDistance = true;
    [Tooltip("Teinte multiplicative vers laquelle tendent les couches lointaines")]
    [SerializeField] private Color distanceTint = new Color(0.75f, 0.82f, 0.95f);
    [Tooltip("Transparence maximale des couches lointaines : laisse transparaître le fond")]
    [SerializeField, Range(0f, 1f)] private float maxFade = 0.5f;
    [Tooltip("Distance caractéristique de l'effet atmosphérique")]
    [SerializeField, Min(0.01f)] private float fogDistance = 40f;
    [SerializeField] private string sortingLayer = "Default";
    [Tooltip("Ordre de rendu de la couche la plus proche ; les suivantes décrémentent")]
    [SerializeField] private int frontSortingOrder = -1;

    [Header("Fonds défilant par décalage d'UV")]
    [SerializeField] private List<UVLayer> uvLayers = new List<UVLayer>();

    [Header("Couches")]
    [SerializeField] private List<Layer> layers = new List<Layer>();
    [Tooltip("0 = tirage différent à chaque lancement")]
    [SerializeField] private int seed = 0;

    private class Element
    {
        public Transform t;
        public SpriteRenderer sr;
        public float minX, maxX; // bords gauche/droit relatifs au pivot, échelle incluse
    }

    private class LayerState
    {
        public Layer def;
        public float factor;      // facteur de vitesse apparente
        public float perspective; // facteur d'échelle (1 si désactivé)
        public readonly List<Element> elements = new List<Element>();
    }

    private readonly List<LayerState> states = new List<LayerState>();
    private System.Random rng;
    private class UVState
    {
        public Material material;
        public float factor;
        public float tileWidth; // largeur monde d'une répétition de la texture
        public Vector2 offset;
    }
    private readonly List<UVState> uvStates = new List<UVState>();
    private Vector3 lastCamPos;
    private float halfViewW, halfViewH;

    private float Factor(float d) => focalDistance / (focalDistance + d);
    private float Range(Vector2 r) => r.x + (float)rng.NextDouble() * (r.y - r.x);

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        if (!cam.orthographic)
            Debug.LogWarning("ParallaxBackground suppose une caméra orthographique.", this);

        rng = seed == 0 ? new System.Random() : new System.Random(seed);
        UpdateViewSize();
        lastCamPos = cam.transform.position;
        if (followCamera) SnapToCamera(lastCamPos);

        // Couches de sprites et fonds UV triés ensemble, de la plus proche à la plus lointaine :
        // l'ordre de rendu découle de la distance, quel que soit le type de couche
        var entries = new List<(float distance, Action<int> build)>();
        foreach (var l in layers)
        {
            var layer = l;
            entries.Add((layer.distance, order => BuildLayer(layer, order)));
        }
        foreach (var u in uvLayers)
        {
            var uv = u;
            if (uv.renderer != null) entries.Add((uv.distance, order => BuildUVLayer(uv, order)));
        }
        entries.Sort((a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < entries.Count; i++)
            entries[i].build(frontSortingOrder - i);
    }

    private void BuildLayer(Layer layer, int order)
    {
        if (layer.sprites == null || layer.sprites.Length == 0) return;

        var st = new LayerState
        {
            def = layer,
            factor = Factor(layer.distance),
            perspective = scaleWithDistance ? Factor(layer.distance) : 1f
        };

        // Perspective atmosphérique : teinte multiplicative et transparence croissantes
        float k = 1f - Mathf.Exp(-layer.distance / fogDistance);
        Color tint = Color.Lerp(Color.white, distanceTint, k);
        tint.a = 1f - maxFade * k;

        var parent = new GameObject(layer.name).transform;
        parent.SetParent(transform, false);

        for (int i = 0; i < layer.count; i++)
        {
            var go = new GameObject($"{layer.name}_{i}");
            go.transform.SetParent(parent, false);
            var e = new Element { t = go.transform, sr = go.AddComponent<SpriteRenderer>() };
            e.sr.color = tint;
            e.sr.sortingLayerName = sortingLayer;
            e.sr.sortingOrder = order;

            // Placement initial en file à partir du bord gauche de la vue
            PlaceRight(e, st, -halfViewW);
            st.elements.Add(e);
        }
        states.Add(st);
    }

    private void BuildUVLayer(UVLayer uv, int order)
    {
        Material mat = uv.renderer.material; // instance propre : chaque fond a son offset
        uvStates.Add(new UVState
        {
            material = mat,
            factor = Factor(uv.distance),
            tileWidth = uv.renderer.bounds.size.x / Mathf.Max(mat.mainTextureScale.x, 1e-4f),
            offset = mat.mainTextureOffset
        });
        uv.renderer.sortingLayerName = sortingLayer;
        uv.renderer.sortingOrder = order;
    }

    private void LateUpdate()
    {
        UpdateViewSize();
        float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector3 camPos = cam.transform.position;

        // Déplacement de référence, à distance nulle : défilement automatique + mouvement caméra
        float scroll = baseSpeed * dt;
        if (followCamera)
        {
            scroll += camPos.x - lastCamPos.x;
            SnapToCamera(camPos);
        }
        lastCamPos = camPos;

        foreach (var st in states)
        {
            float move = scroll * st.factor;
            if (move == 0f) continue;

            foreach (var e in st.elements)
            {
                var p = e.t.localPosition;
                p.x -= move;
                e.t.localPosition = p;

                // Recyclage uniquement du côté par lequel le contenu sort, pour éviter tout va-et-vient
                if (move > 0f && p.x + e.maxX < -halfViewW) PlaceRight(e, st, halfViewW);
                else if (move < 0f && p.x + e.minX > halfViewW) PlaceLeft(e, st, -halfViewW);
            }
        }

        foreach (var u in uvStates)
        {
            u.offset.x = Mathf.Repeat(u.offset.x + scroll * u.factor / u.tileWidth, 1f);
            u.material.mainTextureOffset = u.offset;
        }
    }

    // Tire un nouveau sprite, une échelle, une hauteur et un retournement
    private void Randomize(Element e, LayerState st)
    {
        var L = st.def;
        e.sr.sprite = L.sprites[rng.Next(L.sprites.Length)];
        e.sr.flipX = L.randomFlipX && rng.Next(2) == 0;

        float s = Range(L.scaleRange) * st.perspective;
        e.t.localScale = new Vector3(s, s, 1f);

        // Bords relatifs au pivot, valables quel que soit le pivot du sprite
        Bounds b = e.sr.sprite.bounds;
        float min = b.min.x * s, max = b.max.x * s;
        if (e.sr.flipX) { e.minX = -max; e.maxX = -min; }
        else { e.minX = min; e.maxX = max; }

        float y = Mathf.Lerp(-halfViewH, halfViewH, Range(L.heightRange));
        e.t.localPosition = new Vector3(e.t.localPosition.x, y, 0f);
    }

    private void PlaceRight(Element e, LayerState st, float bound)
    {
        Randomize(e, st);
        float edge = bound;
        foreach (var o in st.elements)
            if (o != e) edge = Mathf.Max(edge, o.t.localPosition.x + o.maxX);
        float gap = Range(st.def.gapRange) * st.perspective;
        SetX(e, edge + gap - e.minX);
    }

    private void PlaceLeft(Element e, LayerState st, float bound)
    {
        Randomize(e, st);
        float edge = bound;
        foreach (var o in st.elements)
            if (o != e) edge = Mathf.Min(edge, o.t.localPosition.x + o.minX);
        float gap = Range(st.def.gapRange) * st.perspective;
        SetX(e, edge - gap - e.maxX);
    }

    private static void SetX(Element e, float x)
    {
        var p = e.t.localPosition;
        p.x = x;
        e.t.localPosition = p;
    }

    private void SnapToCamera(Vector3 camPos)
    {
        transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
    }

    private void UpdateViewSize()
    {
        halfViewH = cam.orthographicSize;
        halfViewW = halfViewH * cam.aspect;
    }
}