using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Train : MonoBehaviour
{
    public static Train instance;

    public float distance;
    public float money;

    public Vector3 FirstPos;
    [SerializeField] private TMP_Text MoneyText;

    [SerializeField] private List<Wagon> WagonPrefab = new List<Wagon>();
    private List<Wagon> WagonList = new List<Wagon>();

    private int SelectedWagon;

    [Header("Réparation (flèche haut, maintenue)")]
    [SerializeField] private float repairRate = 8f;           // PV/s
    [SerializeField] private float repairCostPerHealth = 2f;  // $ par PV

    [Header("Amélioration (flèche bas)")]
    [Tooltip("Coût pour passer au niveau 2, puis 3, etc. Le niveau max en découle.")]
    [SerializeField] private int[] upgradeCosts = { 40, 80 };

    [Header("Affichage")]
    [SerializeField] private float infoLabelHeight = 1.45f;   // au-dessus du centre du wagon sélectionné

    public int MaxLevel => upgradeCosts.Length + 1;
    private Wagon Selected => WagonList.Count > 0 ? WagonList[SelectedWagon] : null;

    private TextMeshPro infoLabel;
    private Color moneyTextColor;
    private float moneyFlashTimer;
    private float repairFxTimer;

    private void Start()
    {
        SpawnTrain();
        instance = this;
        distance = 0;
        money = 0;

        moneyTextColor = MoneyText.color;
        CreateInfoLabel();
    }

    public void SpawnTrain()
    {
        for (int i = 0; i < WagonPrefab.Count; i++)
        {
            InstantiateWagon(i);
        }
    }

    public void InstantiateWagon(int i)
    {
        if (i >= WagonPrefab.Count || i < 0) return;

        Wagon wagon = Instantiate(WagonPrefab[i], transform);

        wagon.train = this;
        wagon.order = WagonList.Count;
        if (i == 0)
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

        // le train se contrôle aux flèches (l'avion est sur ZQSD)
        if (kb.leftArrowKey.wasPressedThisFrame) MoveSelection(+1);
        if (kb.rightArrowKey.wasPressedThisFrame) MoveSelection(-1);
        if (kb.upArrowKey.isPressed) RepairSelected(Time.deltaTime);
        if (kb.downArrowKey.wasPressedThisFrame) UpgradeSelected();

        DisplayMoney();
    }

    private void LateUpdate()
    {
        UpdateInfoLabel();
    }

    // ---------- réparation / amélioration ----------

    public int UpgradeCost(int currentLevel) => upgradeCosts[currentLevel - 1];

    private void RepairSelected(float dt)
    {
        Wagon w = Selected;
        if (w == null || !w.CanAct) return;

        float missing = w.MaxHealth - w.Health;
        if (missing <= 0f) return;

        float amount = Mathf.Min(repairRate * dt, missing, money / repairCostPerHealth);
        if (amount <= 0.0001f)
        {
            NotEnoughMoney();
            return;
        }

        w.Repair(amount);
        money -= amount * repairCostPerHealth;

        // petites étincelles vertes pendant la réparation
        repairFxTimer -= dt;
        if (repairFxTimer <= 0f)
        {
            repairFxTimer = 0.12f;
            Bounds b = w.GetComponent<SpriteRenderer>().bounds;
            Vector3 p = new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
            Explosion.Spawn(p, 0.18f, new Color(0.4f, 1f, 0.4f, 0.9f), 0.3f);
        }
    }

    private void UpgradeSelected()
    {
        Wagon w = Selected;
        if (w == null || !w.CanAct || w.Level >= MaxLevel) return;

        int cost = UpgradeCost(w.Level);
        if (money < cost)
        {
            NotEnoughMoney();
            return;
        }

        money -= cost;
        w.Upgrade();
    }

    private void NotEnoughMoney()
    {
        moneyFlashTimer = 0.3f;
    }
    public void MoveSelection(int delta)
    {
        if (WagonList.Count == 0) return;

        WagonList[SelectedWagon].isSelected = false;

        // on saute les wagons détruits
        int next = SelectedWagon;
        for (int i = 0; i < WagonList.Count; i++)
        {
            next = (next + delta + WagonList.Count) % WagonList.Count;
            if (!WagonList[next].IsDead) break;
        }
        SelectedWagon = next;
        WagonList[SelectedWagon].isSelected = !WagonList[SelectedWagon].IsDead;
    }

    public void OnWagonDestroyed(Wagon wagon)
    {
        if (WagonList.IndexOf(wagon) == SelectedWagon)
            MoveSelection(+1);
    }

    void DisplayMoney()
    {
        MoneyText.SetText("train: $" + money.ToString("0"));

        // le compteur clignote en rouge quand on n'a pas assez d'argent
        if (moneyFlashTimer > 0f) moneyFlashTimer -= Time.deltaTime;
        MoneyText.color = moneyFlashTimer > 0f ? new Color(1f, 0.3f, 0.3f) : moneyTextColor;
    }

    // ---------- étiquette au-dessus du wagon sélectionné ----------

    private void CreateInfoLabel()
    {
        var go = new GameObject("WagonInfoLabel");
        infoLabel = go.AddComponent<TextMeshPro>();
        infoLabel.fontSize = 2.4f;
        infoLabel.alignment = TextAlignmentOptions.Center;
        infoLabel.textWrappingMode = TextWrappingModes.NoWrap;
        infoLabel.rectTransform.sizeDelta = new Vector2(8f, 1f);
        infoLabel.sortingOrder = 60;
        infoLabel.outlineWidth = 0.25f;
        infoLabel.outlineColor = Color.black;
    }

    private void UpdateInfoLabel()
    {
        if (infoLabel == null) return;

        Wagon w = Selected;
        bool visible = w != null && w.CanAct;
        infoLabel.gameObject.SetActive(visible);
        if (!visible) return;

        infoLabel.transform.position = w.transform.position + Vector3.up * infoLabelHeight;

        string repair = w.Health >= w.MaxHealth
            ? "<color=#999>PV au max</color>"
            : $"<color=#7f7>Haut</color> réparer {Cost(repairCostPerHealth, 1f, "$/PV")}";

        string upgrade = w.Level >= MaxLevel
            ? "<color=#999>niveau max</color>"
            : $"<color=#fd5>Bas</color> améliorer {Cost(UpgradeCost(w.Level), UpgradeCost(w.Level), "$")}";

        infoLabel.SetText($"<b>{w.DisplayName}</b>  Nv {w.Level}/{MaxLevel}\n{repair}    {upgrade}");
    }

    // prix en rouge si on ne peut pas se le payer
    private string Cost(float price, float needed, string unit)
    {
        string color = money >= needed ? "#fff" : "#f66";
        return $"<color={color}>{price:0.#}{unit}</color>";
    }
}
