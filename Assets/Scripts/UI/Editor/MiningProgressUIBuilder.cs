using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarterAssets;

namespace Muki.Editor
{
    public static class MiningProgressUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
        private const string BodyFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";
        private const string BoldFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Bold SDF.asset";
        private const string BackgroundSprite = "Assets/StarterAssets/Mobile/UI/UI_Circle_Faded.png";
        private const string FillSprite = "Assets/StarterAssets/Mobile/UI/UI_Circle_Bevel_Base.png";

        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Gray = Hex("#575352");
        private static readonly Color Orange = Hex("#d57f40");

        [MenuItem("Muki/Build Mining Progress Circle")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject old = GameObject.Find("UI_MiningProgress");
            if (old != null) Object.DestroyImmediate(old);

            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFont);
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFont);
            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSprite);
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FillSprite);

            GameObject root = new GameObject("UI_MiningProgress", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(MiningProgressUI));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            RectTransform ringRoot = Rect("MiningProgressRing", root.transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0f, -95f), new Vector2(0f, -95f));
            ringRoot.sizeDelta = new Vector2(112f, 112f);

            Image background = ringRoot.gameObject.AddComponent<Image>();
            background.sprite = backgroundSprite;
            background.color = new Color(Gray.r, Gray.g, Gray.b, .8f);
            background.raycastTarget = false;

            GameObject fillObject = new GameObject("MiningProgressFill", typeof(RectTransform), typeof(Image));
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.SetParent(ringRoot, false);
            fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
            Image fill = fillObject.GetComponent<Image>();
            fill.sprite = fillSprite;
            fill.color = Orange;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = 2;
            fill.fillClockwise = true;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            TMP_Text percent = Text(ringRoot, "0%", boldFont, 21f, Gold, TextAlignmentOptions.Center, new Vector2(.1f, .28f), new Vector2(.9f, .72f));
            TMP_Text caption = Text(root.transform, "[E]  PICANDO", bodyFont, 18f, new Color(Gold.r, Gold.g, Gold.b, .95f), TextAlignmentOptions.Center, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
            caption.rectTransform.sizeDelta = new Vector2(260f, 34f);
            caption.rectTransform.anchoredPosition = new Vector2(0f, -170f);

            MiningProgressUI controller = root.GetComponent<MiningProgressUI>();
            SerializedObject data = new SerializedObject(controller);
            data.FindProperty("group").objectReferenceValue = group;
            data.FindProperty("progressFill").objectReferenceValue = fill;
            data.FindProperty("percentText").objectReferenceValue = percent;
            data.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
            Debug.Log("Muki: círculo de progreso de minería creado y conectado a PlayerMining.");
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            return rect;
        }

        private static TMP_Text Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            RectTransform rect = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max, Vector2.zero, Vector2.zero);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = alignment; text.raycastTarget = false; text.enableWordWrapping = false;
            return text;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
