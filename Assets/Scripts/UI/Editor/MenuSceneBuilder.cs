using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class MenuSceneBuilder
    {
        private static readonly Color Ink = Hex("000000");
        private static readonly Color Gold = Hex("ecbb48");
        private static readonly Color Charcoal = Hex("575352");
        private static readonly Color Terracotta = Hex("d57f40");
        private static readonly Color Brown = Hex("71491f");
        private static readonly Color Red = Hex("ac2711");

        [MenuItem("Muki/Build Main Menu")]
        public static void Build()
        {
            EnsureFolder("Assets/Scenes");
            CreateLevelSelectionScene();
            CreateMainMenuScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Muki: escenas MenuPrincipal y SeleccionarNivel creadas correctamente.");
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();
            CreateMenuCamera();

            GameObject controller = new GameObject("MainMenuController");
            MainMenuController menu = controller.AddComponent<MainMenuController>();

            Canvas canvas = CreateCanvas("MenuCanvas");
            RectTransform root = canvas.transform as RectTransform;
            AddImage("Background", root, Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage("WarmGlow", root, new Color(0.443f, 0.286f, 0.122f, 0.18f), new Vector2(0f, 0f), new Vector2(0.52f, 1f), Vector2.zero, Vector2.zero);

            TMP_FontAsset titleFont = LoadFont("Assets/UI/Tipografia/Titulos/dubellay SDF.asset");
            TMP_FontAsset bodyFont = LoadFont("Assets/UI/Tipografia/Subtitulos/CormorantGaramond-SemiBold SDF.asset");
            TMP_FontAsset regularFont = LoadFont("Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset");

            RectTransform titleBlock = CreateRect("TitleBlock", root, new Vector2(0.085f, 0.35f), new Vector2(0.52f, 0.72f), new Vector2(0, 0), new Vector2(0, 0));
            CreateText("Kicker", titleBlock, "UNA LEYENDA ANDINA", regularFont, 24, Gold, TextAlignmentOptions.Left, new Vector2(0, 0.75f), new Vector2(1, 1));
            TMP_Text title = CreateText("Title", titleBlock, "BAJO\nLA LEYENDA", titleFont, 88, Gold, TextAlignmentOptions.Left, new Vector2(0, 0.05f), new Vector2(1, 0.76f));
            title.characterSpacing = 5f;
            CreateImageLine("AccentLine", titleBlock, Terracotta, new Vector2(0, 0.01f), new Vector2(0.22f, 0.018f));
            CreateText("Caption", titleBlock, "El silencio de la montaña\nguarda más de un secreto.", regularFont, 25, new Color(0.78f, 0.75f, 0.69f), TextAlignmentOptions.Left, new Vector2(0, -0.21f), new Vector2(0.9f, 0.02f));

            RectTransform menuPanel = CreateRect("MenuPanel", root, new Vector2(0.64f, 0.31f), new Vector2(0.91f, 0.7f), new Vector2(0, 0), new Vector2(0, 0));
            CreateText("MenuLabel", menuPanel, "COMENZAR", regularFont, 24, new Color(0.78f, 0.75f, 0.69f), TextAlignmentOptions.Left, new Vector2(0.07f, 0.85f), new Vector2(0.93f, 1));
            CreateImageLine("MenuRule", menuPanel, Brown, new Vector2(0.07f, 0.81f), new Vector2(0.86f, 0.008f));

            Button play = CreateButton("PlayButton", menuPanel, "JUGAR", bodyFont, Gold, new Vector2(0.07f, 0.52f), new Vector2(0.93f, 0.76f));
            Button quit = CreateButton("QuitButton", menuPanel, "SALIR", bodyFont, new Color(0.78f, 0.75f, 0.69f), new Vector2(0.07f, 0.19f), new Vector2(0.93f, 0.43f));
            play.onClick.AddListener(menu.Play);
            quit.onClick.AddListener(menu.Quit);

            CreateText("Footer", root, "MUKI  /  2026", regularFont, 18, Charcoal, TextAlignmentOptions.Right, new Vector2(0.78f, 0.04f), new Vector2(0.93f, 0.08f));

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MenuPrincipal.unity");
        }

        private static void CreateLevelSelectionScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();
            CreateMenuCamera();
            Canvas canvas = CreateCanvas("SeleccionarNivelCanvas");
            RectTransform root = canvas.transform as RectTransform;
            AddImage("Background", root, Ink, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TMP_FontAsset titleFont = LoadFont("Assets/UI/Tipografia/Titulos/dubellay SDF.asset");
            TMP_FontAsset bodyFont = LoadFont("Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset");
            CreateText("Title", root, "SELECCIONAR NIVEL", titleFont, 70, Gold, TextAlignmentOptions.Center, new Vector2(0.2f, 0.55f), new Vector2(0.8f, 0.72f));
            CreateText("Placeholder", root, "Esta pantalla está lista para diseñar los niveles.", bodyFont, 28, new Color(0.78f, 0.75f, 0.69f), TextAlignmentOptions.Center, new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.52f));
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SeleccionarNivel.unity");
        }

        private static Button CreateButton(string name, RectTransform parent, string label, TMP_FontAsset font, Color textColor, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(MenuButtonFeedback));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>(); image.color = new Color(0.34f, 0.325f, 0.32f, 0.24f);
            Outline outline = go.GetComponent<Outline>(); outline.effectColor = Brown; outline.effectDistance = new Vector2(2.5f, 2.5f); outline.useGraphicAlpha = false;
            Button button = go.GetComponent<Button>(); button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors; colors.normalColor = new Color(1, 1, 1, 1); colors.highlightedColor = new Color(1, 1, 1, 1); colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1); button.colors = colors;
            CreateText("Label", rect, label, font, 42, textColor, TextAlignmentOptions.Left, new Vector2(0.09f, 0.12f), new Vector2(0.9f, 0.88f));
            return button;
        }

        private static TMP_Text CreateText(string name, RectTransform parent, string text, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            TMP_Text label = go.GetComponent<TMP_Text>(); label.text = text; label.font = font; label.fontSize = size; label.color = color; label.alignment = alignment; label.enableWordWrapping = false; label.raycastTarget = false;
            return label;
        }

        private static GameObject AddImage(string name, RectTransform parent, Color color, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image)); RectTransform rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; go.GetComponent<Image>().color = color; return go;
        }

        private static void CreateImageLine(string name, RectTransform parent, Color color, Vector2 min, Vector2 size)
        { AddImage(name, parent, color, min, min + size, Vector2.zero, Vector2.zero); }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        { GameObject go = new GameObject(name, typeof(RectTransform)); RectTransform rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax; return rect; }

        private static Canvas CreateCanvas(string name)
        { GameObject go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); Canvas canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; CanvasScaler scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; return canvas; }
        private static void CreateMenuCamera()
        {
            GameObject go = new GameObject("Main Camera", typeof(Camera));
            go.tag = "MainCamera";
            Camera camera = go.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Ink;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }
        private static void CreateEventSystem() { new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); }
        private static TMP_FontAsset LoadFont(string path) => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color color); return color; }
        private static void EnsureFolder(string path) { if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets", "Scenes"); }
        private static void UpdateBuildSettings() { EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/MenuPrincipal.unity", true), new EditorBuildSettingsScene("Assets/Scenes/SeleccionarNivel.unity", true), new EditorBuildSettingsScene("Assets/Scenes/EscenaJuego.unity", true) }; }
    }
}
