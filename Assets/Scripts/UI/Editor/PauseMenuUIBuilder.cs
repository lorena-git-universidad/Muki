using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class PauseMenuUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
        private const string TitleFontPath = "Assets/UI/Tipografia/Titulos/dubellay SDF.asset";
        private const string BodyFontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";
        private const string BoldFontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Bold SDF.asset";

        private static readonly Color Ink = Hex("#000000");
        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Orange = Hex("#d57f40");
        private static readonly Color Brown = Hex("#71491f");
        private static readonly Color Cream = Hex("#F0D9A2");

        [MenuItem("Muki/Build Pause Menu UI")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject old = GameObject.Find("PauseMenuCanvas");
            if (old != null) Object.DestroyImmediate(old);

            EnsureEventSystem();
            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);

            GameObject canvasObject = Rect("PauseMenuCanvas", null, Vector2.zero, Vector2.one);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            PauseMenuController controller = canvasObject.AddComponent<PauseMenuController>();

            GameObject pausePanel = Rect("PausePanel", canvasObject.transform, Vector2.zero, Vector2.one);
            GameObject overlay = Rect("Overlay", pausePanel.transform, Vector2.zero, Vector2.one);
            overlay.AddComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, .78f);

            GameObject panel = Rect("Panel_Pausa", pausePanel.transform, new Vector2(.31f, .13f), new Vector2(.69f, .87f));
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(Brown.r, Brown.g, Brown.b, .95f);
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(Orange.r, Orange.g, Orange.b, .9f);
            panelOutline.effectDistance = new Vector2(3f, 3f);

            Text(panel.transform, "PAUSA", titleFont, 72f, Gold, TextAlignmentOptions.Center, new Vector2(.08f, .78f), new Vector2(.92f, .96f));
            GameObject line = Rect("Separador", panel.transform, new Vector2(.18f, .755f), new Vector2(.82f, .765f));
            line.AddComponent<Image>().color = Orange;
            Text(panel.transform, "La exploración está detenida", bodyFont, 24f, Cream, TextAlignmentOptions.Center, new Vector2(.08f, .68f), new Vector2(.92f, .75f));

            GameObject buttons = Rect("Botones", panel.transform, new Vector2(.15f, .20f), new Vector2(.85f, .64f));
            PauseButton(buttons.transform, "Reanudar_Button", "REANUDAR", titleFont, controller.Resume, new Vector2(0f, .68f), new Vector2(1f, 1f));
            PauseButton(buttons.transform, "MenuPrincipal_Button", "VOLVER AL MENÚ PRINCIPAL", bodyFont, controller.GoToMainMenu, new Vector2(0f, .35f), new Vector2(1f, .60f));
            PauseButton(buttons.transform, "Salir_Button", "SALIR DEL JUEGO", bodyFont, controller.QuitGame, new Vector2(0f, 0f), new Vector2(1f, .25f));
            Text(panel.transform, "ESC  ·  CONTINUAR", boldFont, 18f, Cream, TextAlignmentOptions.Center, new Vector2(.08f, .055f), new Vector2(.92f, .12f));

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            pausePanel.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = canvasObject;
            Debug.Log("Muki: pantalla de pausa creada en EscenaPrueba con Escape y tres acciones.");
        }

        private static Button PauseButton(Transform parent, string name, string label, TMP_FontAsset font, UnityEngine.Events.UnityAction action, Vector2 min, Vector2 max)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(MenuButtonFeedback));
            RectTransform rect = buttonObject.transform as RectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(Ink.r, Ink.g, Ink.b, .35f);
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = Brown; outline.effectDistance = new Vector2(2.5f, 2.5f); outline.useGraphicAlpha = false;
            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1f, .9f, .65f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.85f, .65f, .35f, 1f);
            button.colors = colors;
            Text(rect, label, font, name == "Reanudar_Button" ? 34f : 25f, Gold, TextAlignmentOptions.Center, new Vector2(.04f, .1f), new Vector2(.96f, .9f));
            UnityEventTools.AddPersistentListener(button.onClick, action);
            return button;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.transform as RectTransform;
            if (parent != null) rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
            return go;
        }

        private static TextMeshProUGUI Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject go = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = alignment; text.enableWordWrapping = true; text.raycastTarget = false; text.margin = new Vector4(4f, 2f, 4f, 2f);
            return text;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
