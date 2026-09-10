using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarterAssets;

public static class PrototypeUIBuilder
{
    private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
    private const string TitleFont = "Assets/UI/Tipografia/Titulos/dubellit SDF.asset";
    private const string BodyFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";
    private const string BoldFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Bold SDF.asset";

    private static Color32 Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }

    private static readonly Color Black = Hex("#000000");
    private static readonly Color Gold = Hex("#ecbb48");
    private static readonly Color Gray = Hex("#575352");
    private static readonly Color Orange = Hex("#d57f40");
    private static readonly Color Brown = Hex("#71491f");
    private static readonly Color Red = Hex("#ac2711");

    [MenuItem("Muki/Build Mineral Prototype UI")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject existing = GameObject.Find("UI_PrototipoMinerales");
        if (existing != null) Object.DestroyImmediate(existing);

        TMP_FontAsset title = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFont);
        TMP_FontAsset body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFont);
        TMP_FontAsset bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFont);

        GameObject root = Rect("UI_PrototipoMinerales", null, Vector2.zero, Vector2.one);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        root.AddComponent<GraphicRaycaster>();
        PrototypeMineralUI controller = root.AddComponent<PrototypeMineralUI>();

        GameObject panel = Rect("Panel_InventarioMinerales", root.transform, new Vector2(.07f, .06f), new Vector2(.93f, .94f));
        panel.AddComponent<Image>().color = Brown;
        GameObject atmosphere = Rect("Atmosfera", panel.transform, Vector2.zero, Vector2.one);
        atmosphere.AddComponent<Image>().color = new Color(Black.r, Black.g, Black.b, .72f);
        controller.panel = panel;

        GameObject header = Rect("Cabecera", panel.transform, new Vector2(.035f, .89f), new Vector2(.965f, .97f));
        header.AddComponent<Image>().color = Black;
        Text(header.transform, "MINERALES DE MUKI", title, 42, Gold, TextAlignmentOptions.Left, new Vector2(.03f, .08f), new Vector2(.64f, .92f));
        Text(header.transform, "TAB  ·  CERRAR", bold, 22, Orange, TextAlignmentOptions.Right, new Vector2(.70f, .08f), new Vector2(.97f, .92f));

        GameObject left = Rect("Seccion_Minerales", panel.transform, new Vector2(.035f, .065f), new Vector2(.59f, .865f));
        left.AddComponent<Image>().color = Gray;
        Text(left.transform, "MINERALES", title, 34, Gold, TextAlignmentOptions.Left, new Vector2(.05f, .91f), new Vector2(.95f, .995f));
        Text(left.transform, "Recursos descubiertos en la mina", body, 20, Hex("#F0D9A2"), TextAlignmentOptions.Left, new Vector2(.05f, .84f), new Vector2(.95f, .92f));

        GameObject grid = Rect("Grid_Minerales", left.transform, new Vector2(.04f, .045f), new Vector2(.96f, .82f));
        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;
        layout.cellSize = new Vector2(280, 250);
        layout.spacing = new Vector2(18, 18);
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childAlignment = TextAnchor.MiddleCenter;

        string[] names = { "Piedra", "Carbon", "Oro", "Esmeralda", "Diamante" };
        foreach (string name in names)
        {
            string path = name == "Carbon" ? "Assets/Items/Carbon.asset" : $"Assets/Items/{name}.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            GameObject card = Rect("Mineral_" + name, grid.transform);
            card.AddComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, .18f);

            GameObject accent = Rect("Acento", card.transform, new Vector2(0f, .965f), new Vector2(1f, 1f));
            accent.AddComponent<Image>().color = Orange;

            GameObject iconFrame = Rect("MarcoIcono", card.transform, new Vector2(.23f, .56f), new Vector2(.77f, .88f));
            iconFrame.AddComponent<Image>().color = new Color(Brown.r, Brown.g, Brown.b, .58f);

            GameObject iconObject = Rect("Icono", iconFrame.transform, new Vector2(0f, .06f), new Vector2(1f, .94f));
            Image icon = iconObject.AddComponent<Image>();
            icon.sprite = item == null ? null : item.icon;
            icon.preserveAspect = true;
            TMP_Text nameText = Text(card.transform, item == null ? name.ToUpperInvariant() : item.itemName.ToUpperInvariant(), bold, 26, Black, TextAlignmentOptions.Center, new Vector2(.03f, .37f), new Vector2(.97f, .50f));

            GameObject divider = Rect("Separador", card.transform, new Vector2(.22f, .35f), new Vector2(.78f, .365f));
            divider.AddComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, .7f);

            GameObject quantityBackground = Rect("FondoCantidad", card.transform, new Vector2(.80f, .80f), new Vector2(.95f, .95f));
            quantityBackground.AddComponent<Image>().color = new Color(Red.r, Red.g, Red.b, .86f);
            TMP_Text quantityText = Text(quantityBackground.transform, "0", bold, 30, Gold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            PrototypeMineralUI.MineralEntry entry = new();
            entry.itemData = item;
            entry.iconFrame = iconFrame;
            entry.quantityContainer = quantityBackground;
            entry.divider = divider;
            entry.iconImage = icon;
            entry.cardRoot = card;
            entry.nameText = nameText;
            entry.quantityText = quantityText;
            controller.mineralEntries.Add(entry);
        }

        GameObject right = Rect("Seccion_Objetivos", panel.transform, new Vector2(.615f, .065f), new Vector2(.965f, .865f));
        right.AddComponent<Image>().color = Gray;
        Text(right.transform, "OBJETIVOS", title, 34, Gold, TextAlignmentOptions.Left, new Vector2(.08f, .91f), new Vector2(.92f, .995f));
        Text(right.transform, "Tu camino se revela paso a paso", body, 20, Hex("#F0D9A2"), TextAlignmentOptions.Left, new Vector2(.08f, .84f), new Vector2(.92f, .92f));
        controller.objectiveProgressText = Objective(right.transform, "01", "Recolecta minerales", "Encuentra tres minerales distintos en la mina.", "EN PROGRESO", body, bold, Gold);
        Objective(right.transform, "02", "Entrega la ofrenda", "Lleva tus minerales al altar de la montaña.", "BLOQUEADO", body, bold, Orange);
        Objective(right.transform, "03", "Descubre la salida", "Sigue las señales y encuentra el camino de regreso.", "BLOQUEADO", body, bold, Red);

        Text(panel.transform, "Pulsa TAB para volver a la exploración", bold, 20, Hex("#F0D9A2"), TextAlignmentOptions.Center, new Vector2(.035f, .012f), new Vector2(.965f, .045f));
        panel.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = panel;
        Debug.Log("Muki: UI de minerales actualizada con barras y estado de desbloqueo.");
    }

    private static TMP_Text Objective(Transform parent, string number, string heading, string description, string status, TMP_FontAsset body, TMP_FontAsset bold, Color statusColor)
    {
        int index = int.Parse(number) - 1;
        GameObject objective = Rect("Objetivo_" + number, parent, new Vector2(.08f, .57f - index * .20f), new Vector2(.92f, .75f - index * .20f));
        objective.AddComponent<Image>().color = new Color(Black.r, Black.g, Black.b, .32f);
        Text(objective.transform, number, bold, 27, Gold, TextAlignmentOptions.Center, new Vector2(.03f, .55f), new Vector2(.18f, .95f));
        Text(objective.transform, heading, bold, 23, Gold, TextAlignmentOptions.Left, new Vector2(.22f, .60f), new Vector2(.94f, .92f));
        Text(objective.transform, description, body, 17, Hex("#F0D9A2"), TextAlignmentOptions.Left, new Vector2(.22f, .28f), new Vector2(.94f, .62f));
        return Text(objective.transform, status, bold, 16, statusColor, TextAlignmentOptions.Left, new Vector2(.22f, .05f), new Vector2(.94f, .28f));
    }

    private static GameObject Rect(string name, Transform parent, Vector2 min = default, Vector2 max = default)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        return go;
    }

    private static TextMeshProUGUI Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
    {
        GameObject go = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        text.margin = new Vector4(4, 2, 4, 2);
        return text;
    }
}
