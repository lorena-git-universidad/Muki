using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class SelectionLevelUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/SeleccionarNivel.unity";
        private const string TitleFont = "Assets/UI/Tipografia/Titulos/dubellay SDF.asset";
        private const string BodyFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";
        private const string BoldFont = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Bold SDF.asset";

        private static readonly Color Ink = Hex("#000000");
        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Gray = Hex("#575352");
        private static readonly Color Orange = Hex("#d57f40");
        private static readonly Color Brown = Hex("#71491f");
        private static readonly Color Red = Hex("#ac2711");

        [MenuItem("Muki/Build SeleccionarNivel UI")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject old = GameObject.Find("UI_SeleccionNivel");
            if (old != null) Object.DestroyImmediate(old);

            Canvas canvas = GameObject.Find("SeleccionarNivelCanvas")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No se encontró SeleccionarNivelCanvas.");
                return;
            }

            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFont);
            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFont);
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFont);

            GameObject root = Rect("UI_SeleccionNivel", canvas.transform, Vector2.zero, Vector2.one);
            PrototypeSceneMenuController controller = root.AddComponent<PrototypeSceneMenuController>();
            PrototypeSceneIntroAnimation intro = root.AddComponent<PrototypeSceneIntroAnimation>();

            GameObject panel = Rect("Panel_SeleccionLeyenda", root.transform, new Vector2(.27f, .16f), new Vector2(.73f, .57f));
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(Brown.r, Brown.g, Brown.b, .22f);
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(Orange.r, Orange.g, Orange.b, .72f);
            panelOutline.effectDistance = new Vector2(2f, 2f);

            TMP_Text heading = Text(panel.transform, "ELIGE TU LEYENDA", boldFont, 26f, Gold, TextAlignmentOptions.Center, new Vector2(.08f, .80f), new Vector2(.92f, .96f));
            GameObject rule = Rect("Linea_Seleccion", panel.transform, new Vector2(.18f, .76f), new Vector2(.82f, .77f));
            rule.AddComponent<Image>().color = Orange;

            Button muki = CreateButton("Muki_Button", panel.transform, "MUKI", titleFont, Gold, new Vector2(.14f, .45f), new Vector2(.86f, .69f));
            Button upcoming = CreateButton("Proximamente_Button", panel.transform, "PRÓXIMAMENTE", bodyFont, Gray, new Vector2(.14f, .16f), new Vector2(.86f, .36f));
            upcoming.interactable = false;
            upcoming.GetComponent<Image>().color = new Color(Gray.r, Gray.g, Gray.b, .18f);
            upcoming.GetComponent<Outline>().effectColor = new Color(Gray.r, Gray.g, Gray.b, .7f);
            upcoming.GetComponent<MenuButtonFeedback>().enabled = false;

            UnityEventTools.AddPersistentListener(muki.onClick, controller.OpenPrototype);

            GameObject tooltipObject = Rect("Muki_HoverTooltip", panel.transform, new Vector2(.14f, .04f), new Vector2(.86f, .14f));
            Image tooltipImage = tooltipObject.AddComponent<Image>();
            tooltipImage.color = new Color(Ink.r, Ink.g, Ink.b, .86f);
            CanvasGroup tooltipGroup = tooltipObject.AddComponent<CanvasGroup>();
            TMP_Text tooltipText = Text(tooltipObject.transform, "Muki guarda una leyenda en cada rincón.", bodyFont, 19f, new Color(Gold.r, Gold.g, Gold.b, .95f), TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
            tooltipText.fontStyle = FontStyles.Italic;
            PrototypeSceneTooltip tooltip = muki.gameObject.AddComponent<PrototypeSceneTooltip>();
            tooltip.Configure(tooltipGroup, tooltipText, "Muki guarda una leyenda en cada rincón.");

            SerializedObject serializedIntro = new SerializedObject(intro);
            serializedIntro.FindProperty("title").objectReferenceValue = heading.GetComponent<CanvasGroup>() ?? heading.gameObject.AddComponent<CanvasGroup>();
            serializedIntro.FindProperty("playButton").objectReferenceValue = muki.gameObject.AddComponent<CanvasGroup>();
            serializedIntro.FindProperty("upcomingButton").objectReferenceValue = upcoming.gameObject.AddComponent<CanvasGroup>();
            serializedIntro.FindProperty("tooltip").objectReferenceValue = tooltipGroup;
            serializedIntro.FindProperty("titleRect").objectReferenceValue = heading.rectTransform;
            serializedIntro.FindProperty("playRect").objectReferenceValue = muki.transform as RectTransform;
            serializedIntro.FindProperty("upcomingRect").objectReferenceValue = upcoming.transform as RectTransform;
            serializedIntro.ApplyModifiedPropertiesWithoutUndo();

            // El texto existente conserva la jerarquía y la escena; solo se actualiza para acompañar la nueva IU.
            GameObject placeholder = GameObject.Find("Placeholder");
            if (placeholder != null)
            {
                TMP_Text placeholderText = placeholder.GetComponent<TMP_Text>();
                if (placeholderText != null) placeholderText.text = "Escoge una leyenda para comenzar tu aventura.";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
            Debug.Log("Muki: UI de SeleccionarNivel creada con botones visibles en la jerarquía.");
        }

        private static Button CreateButton(string name, Transform parent, string label, TMP_FontAsset font, Color textColor, Vector2 min, Vector2 max)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(MenuButtonFeedback));
            RectTransform rect = buttonObject.transform as RectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = new Color(Brown.r, Brown.g, Brown.b, .64f);
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = Brown; outline.effectDistance = new Vector2(2.5f, 2.5f); outline.useGraphicAlpha = false;
            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            Text(rect, label, font, label == "MUKI" ? 45f : 25f, textColor, TextAlignmentOptions.Center, new Vector2(.05f, .1f), new Vector2(.95f, .9f));
            return button;
        }

        private static TMP_Text Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject textObject = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = alignment; text.raycastTarget = false; text.enableWordWrapping = true;
            return text;
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.transform as RectTransform;
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
            return go;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
