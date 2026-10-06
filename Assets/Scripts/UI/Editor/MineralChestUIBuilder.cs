using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarterAssets;

namespace Muki.Editor
{
    public static class MineralChestUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
        private const string BodyFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";

        private static readonly Color Ink = Hex("#000000");
        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Gray = Hex("#575352");
        private static readonly Color Orange = Hex("#d57f40");
        private static readonly Color Brown = Hex("#71491f");
        private static readonly Color Red = Hex("#ac2711");

        [MenuItem("Muki/Improve Mineral Chest UI")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            MineralChest chest = Object.FindFirstObjectByType<MineralChest>(FindObjectsInactive.Include);
            if (chest == null)
            {
                Debug.LogError("No se encontró un objeto MineralChest en EscenaPrueba.");
                return;
            }

            Transform oldVisual = chest.transform.Find("CofreUI_Visual");
            if (oldVisual != null) Object.DestroyImmediate(oldVisual.gameObject);

            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFont);

            GameObject root = new GameObject("CofreUI_Visual", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(ChestUIBillboard), typeof(ChestUIVisual));
            root.transform.SetParent(chest.transform, false);
            root.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            root.transform.localScale = Vector3.one * 0.0028f;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(360f, 165f);

            GameObject panel = Rect("Panel", root.transform, Vector2.zero, Vector2.one);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(Ink.r, Ink.g, Ink.b, .84f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(Brown.r, Brown.g, Brown.b, .95f);
            outline.effectDistance = new Vector2(5f, 5f);

            GameObject accent = Rect("Acento_Dorado", panel.transform, new Vector2(.04f, .90f), new Vector2(.96f, .94f));
            accent.AddComponent<Image>().color = Gold;
            TMP_Text count = Text(panel.transform, "0/0", bodyFont, 52f, Gold, TextAlignmentOptions.Center, new Vector2(.05f, .38f), new Vector2(.95f, .73f));

            GameObject barBackground = Rect("Barra_Fondo", panel.transform, new Vector2(.08f, .27f), new Vector2(.92f, .34f));
            barBackground.AddComponent<Image>().color = new Color(Gray.r, Gray.g, Gray.b, .75f);
            GameObject fillObject = Rect("Barra_Progreso", barBackground.transform, Vector2.zero, Vector2.one);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = Orange;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;

            TMP_Text hint = Text(panel.transform, "[E]  DEPOSITAR", bodyFont, 16f, new Color(0.78f, 0.75f, 0.69f, .95f), TextAlignmentOptions.Center, new Vector2(.08f, .08f), new Vector2(.92f, .23f));
            hint.gameObject.name = "Texto_E_Depositar";

            ChestUIVisual visual = root.GetComponent<ChestUIVisual>();
            SerializedObject visualData = new SerializedObject(visual);
            visualData.FindProperty("countText").objectReferenceValue = count;
            visualData.FindProperty("detailText").objectReferenceValue = null;
            visualData.FindProperty("progressFill").objectReferenceValue = fill;
            visualData.FindProperty("canvasGroup").objectReferenceValue = root.AddComponent<CanvasGroup>();
            visualData.ApplyModifiedPropertiesWithoutUndo();

            MineralChestUI chestUI = chest.chestUI;
            if (chestUI != null)
            {
                chestUI.visual = visual;
                if (chestUI.mineralText != null)
                    chestUI.mineralText.gameObject.SetActive(false);
                EditorUtility.SetDirty(chestUI);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
            Debug.Log("Muki: UI visual del cofre mejorada sin alterar la lógica de minerales.");
        }

        private static TMP_Text Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject go = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = alignment; text.raycastTarget = false; text.enableWordWrapping = false;
            return text;
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            return go;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
