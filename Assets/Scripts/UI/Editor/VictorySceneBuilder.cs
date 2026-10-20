using System.Collections.Generic;
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
    /// <summary>
    /// Pantalla de victoria con la misma composición del menú principal:
    /// título dorado a la izquierda, riel central y panel de acciones a la derecha.
    /// </summary>
    public static class VictorySceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Victoria.unity";
        private const string TitleFontPath = "Assets/UI/Tipografia/Titulos/dubellay SDF.asset";
        private const string BodyFontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset";
        private const string BoldFontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Bold SDF.asset";

        private static readonly Color Ink = Hex("#000000");
        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Gray = Hex("#575352");
        private static readonly Color Orange = Hex("#d57f40");
        private static readonly Color Brown = Hex("#71491f");
        private static readonly Color Cream = Hex("#C7BFAF");

        [MenuItem("Muki/Build Victory Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.cullingMask = 0;

            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            GameObject canvasObject = Rect("VictoryCanvas", null, Vector2.zero, Vector2.one);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            VictoryMenuController controller = canvasObject.AddComponent<VictoryMenuController>();

            // ---------- Fondo ----------
            GameObject background = Rect("Fondo", canvasObject.transform, Vector2.zero, Vector2.one);
            background.AddComponent<Image>().color = Ink;
            Block(background.transform, "Resplandor_Calido", new Color(Brown.r, Brown.g, Brown.b, .20f), new Vector2(0f, 0f), new Vector2(.52f, 1f));
            Block(background.transform, "Resplandor_Centro", new Color(Brown.r, Brown.g, Brown.b, .10f), new Vector2(.52f, 0f), new Vector2(.62f, 1f));
            Block(background.transform, "Viñeta_Superior", new Color(Ink.r, Ink.g, Ink.b, .55f), new Vector2(0f, .90f), new Vector2(1f, 1f));
            Block(background.transform, "Viñeta_Inferior", new Color(Ink.r, Ink.g, Ink.b, .55f), new Vector2(0f, 0f), new Vector2(1f, .10f));

            // ---------- Riel central ----------
            GameObject rail = Block(canvasObject.transform, "Riel_Central", new Color(Orange.r, Orange.g, Orange.b, .7f), new Vector2(.605f, .13f), new Vector2(.607f, .87f));

            // ---------- Bloque izquierdo (título) ----------
            GameObject titleBlock = GroupRect("Bloque_Titulo", canvasObject.transform, Vector2.zero, Vector2.one, out CanvasGroup titleGroup);
            Text(titleBlock.transform, "CAPÍTULO I  /  EL DESENLACE", boldFont, 18f, new Color(Orange.r, Orange.g, Orange.b, .9f), TextAlignmentOptions.Left, new Vector2(.085f, .735f), new Vector2(.54f, .775f), 4f);
            TextMeshProUGUI title = Text(titleBlock.transform, "¡ESCAPASTE!", titleFont, 124f, Gold, TextAlignmentOptions.Left, new Vector2(.085f, .50f), new Vector2(.58f, .73f), 7f);
            title.lineSpacing = -8f;
            Block(titleBlock.transform, "Linea_Titulo", new Color(Gold.r, Gold.g, Gold.b, .55f), new Vector2(.09f, .485f), new Vector2(.30f, .487f));
            TextMeshProUGUI caption = Text(titleBlock.transform, "Saliste de la mina con vida.\nPero la montaña guarda más de un secreto...", bodyFont, 30f, Cream, TextAlignmentOptions.Left, new Vector2(.085f, .33f), new Vector2(.54f, .46f), 0f);
            caption.fontStyle = FontStyles.Italic;
            Text(titleBlock.transform, "MUKI  /  01", boldFont, 16f, Gray, TextAlignmentOptions.Left, new Vector2(.085f, .07f), new Vector2(.34f, .10f), 3f);

            // ---------- Bloque derecho (acciones) ----------
            GameObject menuBlock = GroupRect("Bloque_Acciones", canvasObject.transform, Vector2.zero, Vector2.one, out CanvasGroup menuGroup);
            Text(menuBlock.transform, "TU HISTORIA CONTINÚA", boldFont, 17f, Gray, TextAlignmentOptions.Left, new Vector2(.64f, .755f), new Vector2(.91f, .79f), 4f);
            Block(menuBlock.transform, "Linea_Panel", new Color(Gold.r, Gold.g, Gold.b, .42f), new Vector2(.64f, .735f), new Vector2(.91f, .737f));

            GameObject panel = Rect("Panel_Victoria", menuBlock.transform, new Vector2(.64f, .24f), new Vector2(.91f, .72f));
            panel.AddComponent<Image>().color = new Color(Brown.r, Brown.g, Brown.b, .22f);
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(Orange.r, Orange.g, Orange.b, .72f);
            panelOutline.effectDistance = new Vector2(2f, 2f);

            Text(panel.transform, "¿Y AHORA QUÉ?", bodyFont, 24f, new Color(Cream.r, Cream.g, Cream.b, .85f), TextAlignmentOptions.Center, new Vector2(.08f, .84f), new Vector2(.92f, .96f), 3f);

            Button restart = MenuButton(panel.transform, "Reiniciar_Button", "JUGAR DE NUEVO", titleFont, 38f, Gold, new Color(Brown.r, Brown.g, Brown.b, .48f), Orange, 2.5f, new Vector2(.09f, .54f), new Vector2(.91f, .80f));
            AddAccent(restart.transform, Orange);
            Button menu = MenuButton(panel.transform, "MenuPrincipal_Button", "VOLVER AL INICIO", bodyFont, 26f, Cream, new Color(Gray.r, Gray.g, Gray.b, .14f), Brown, 1.5f, new Vector2(.09f, .30f), new Vector2(.91f, .49f));
            Button quit = MenuButton(panel.transform, "Salir_Button", "SALIR DEL JUEGO", bodyFont, 26f, Cream, new Color(Gray.r, Gray.g, Gray.b, .14f), Brown, 1.5f, new Vector2(.09f, .07f), new Vector2(.91f, .26f));
            UnityEventTools.AddPersistentListener(restart.onClick, controller.RestartLevel);
            UnityEventTools.AddPersistentListener(menu.onClick, controller.GoToMainMenu);
            UnityEventTools.AddPersistentListener(quit.onClick, controller.QuitGame);

            Text(menuBlock.transform, "ENTER  ·  JUGAR DE NUEVO       ESC  ·  INICIO", boldFont, 16f, new Color(Cream.r, Cream.g, Cream.b, .58f), TextAlignmentOptions.Left, new Vector2(.64f, .085f), new Vector2(.93f, .115f), 2f);

            // ---------- Conexión con el controlador ----------
            SerializedObject serialized = new SerializedObject(controller);
            SerializedProperty reveals = serialized.FindProperty("reveals");
            reveals.arraySize = 2;
            SetReveal(reveals.GetArrayElementAtIndex(0), titleGroup, 0.5f);
            SetReveal(reveals.GetArrayElementAtIndex(1), menuGroup, 1.1f);
            serialized.FindProperty("rail").objectReferenceValue = rail.GetComponent<RectTransform>();
            serialized.FindProperty("firstButton").objectReferenceValue = restart;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            Selection.activeGameObject = canvasObject;
            Debug.Log("Muki: escena Victoria creada y agregada a Build Settings.");
        }

        private static void SetReveal(SerializedProperty item, CanvasGroup group, float delay)
        {
            item.FindPropertyRelative("group").objectReferenceValue = group;
            item.FindPropertyRelative("delay").floatValue = delay;
            item.FindPropertyRelative("duration").floatValue = 0.9f;
            item.FindPropertyRelative("riseDistance").floatValue = 28f;
        }

        private static void AddToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
                if (s.path == ScenePath) return;
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddAccent(Transform button, Color color)
        {
            GameObject accent = Rect("Acento_Boton", button, new Vector2(0f, 0f), new Vector2(.012f, 1f));
            accent.AddComponent<Image>().color = color;
            accent.GetComponent<Image>().raycastTarget = false;
        }

        private static Button MenuButton(Transform parent, string name, string label, TMP_FontAsset font, float size, Color textColor, Color fill, Color border, float borderSize, Vector2 min, Vector2 max)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(MenuButtonFeedback));
            RectTransform rect = buttonObject.transform as RectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = fill;
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = border; outline.effectDistance = new Vector2(borderSize, borderSize); outline.useGraphicAlpha = false;
            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, .9f, .65f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.85f, .65f, .35f, 1f);
            button.colors = colors;
            Text(rect, label, font, size, textColor, TextAlignmentOptions.Center, new Vector2(.04f, .1f), new Vector2(.96f, .9f), 2f);
            return button;
        }

        private static GameObject GroupRect(string name, Transform parent, Vector2 min, Vector2 max, out CanvasGroup group)
        {
            GameObject go = Rect(name, parent, min, max);
            group = go.AddComponent<CanvasGroup>();
            return go;
        }

        private static GameObject Block(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = Rect(name, parent, min, max);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.transform as RectTransform;
            if (parent != null) rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
            return go;
        }

        private static TextMeshProUGUI Text(Transform parent, string value, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, float spacing)
        {
            GameObject go = Rect("Texto_" + value.Split('\n')[0].Replace(" ", "_"), parent, min, max);
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value; text.font = font; text.fontSize = size; text.color = color; text.alignment = alignment;
            text.characterSpacing = spacing;
            text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false; text.margin = new Vector4(4f, 2f, 4f, 2f);
            return text;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }
    }
}
