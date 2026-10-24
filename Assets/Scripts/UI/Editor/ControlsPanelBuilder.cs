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
    /// <summary>
    /// Crea la pantalla "Cómo se juega" en EscenaPrueba (visible al empezar el nivel) y añade el botón
    /// "CÓMO SE JUEGA" al menú de pausa. Se puede ejecutar varias veces: reemplaza lo anterior.
    /// </summary>
    public static class ControlsPanelBuilder
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

        private struct Row
        {
            public string key, title, detail;
            public float units;
            public Row(string key, string title, string detail = null, float units = 1f)
            {
                this.key = key; this.title = title; this.detail = detail; this.units = units;
            }
        }

        private static readonly Row[] Rows =
        {
            new Row("W  A  S  D", "Moverte"),
            new Row("SHIFT", "Correr"),
            new Row("C", "Agacharte"),
            new Row("E", "Interactuar",
                "Mantenla para picar  ·  Radio  ·  Esconderte  ·  Poner y recoger ofrendas  ·  Dejar minerales en el cofre", 2f),
            new Row("TAB", "Ver el inventario de minerales"),
            new Row("M", "Abrir o cerrar el minimapa"),
            new Row("ESC", "Pausa"),
        };

        [MenuItem("Muki/Build Controls Panel")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "ControlsCanvas") Object.DestroyImmediate(root);

            TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFontPath);
            TMP_FontAsset boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);

            // ---------- Canvas (siempre activo en la jerarquía; solo se oculta su contenido) ----------
            GameObject canvasObject = Rect("ControlsCanvas", null, Vector2.zero, Vector2.one);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110; // por encima del menú de pausa (100)
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            ControlsPanel controls = canvasObject.AddComponent<ControlsPanel>();

            GameObject contentRoot = Rect("Controles_Panel", canvasObject.transform, Vector2.zero, Vector2.one);
            GameObject overlay = Rect("Overlay", contentRoot.transform, Vector2.zero, Vector2.one);
            overlay.AddComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, .86f);

            GameObject panel = Rect("Panel_Controles", contentRoot.transform, new Vector2(.20f, .07f), new Vector2(.80f, .93f));
            panel.AddComponent<Image>().color = new Color(Brown.r, Brown.g, Brown.b, .95f);
            Outline panelOutline = panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(Orange.r, Orange.g, Orange.b, .9f);
            panelOutline.effectDistance = new Vector2(3f, 3f);

            Text(panel.transform, "CÓMO SE JUEGA", titleFont, 66f, Gold, TextAlignmentOptions.Center, new Vector2(.06f, .86f), new Vector2(.94f, .98f), 3f);
            GameObject line = Rect("Separador", panel.transform, new Vector2(.18f, .845f), new Vector2(.82f, .853f));
            line.AddComponent<Image>().color = Orange;

            // ---------- Filas de controles ----------
            GameObject rowsArea = Rect("Controles", panel.transform, new Vector2(.07f, .20f), new Vector2(.93f, .82f));
            float totalUnits = 0f;
            foreach (Row r in Rows) totalUnits += r.units;

            float top = 1f;
            foreach (Row r in Rows)
            {
                float h = r.units / totalUnits;
                BuildRow(rowsArea.transform, r, top, top - h, bodyFont, boldFont);
                top -= h;
            }

            // ---------- Botón y ayuda ----------
            Button closeButton = CloseButton(panel.transform, bodyFont, out TMP_Text closeLabel);
            TMP_Text hint = Text(panel.transform, "ENTER  ·  COMENZAR", boldFont, 18f, Cream, TextAlignmentOptions.Center, new Vector2(.08f, .025f), new Vector2(.92f, .075f), 3f);
            UnityEventTools.AddPersistentListener(closeButton.onClick, controls.Close);

            SerializedObject serialized = new SerializedObject(controls);
            serialized.FindProperty("root").objectReferenceValue = contentRoot;
            serialized.FindProperty("closeLabel").objectReferenceValue = closeLabel;
            serialized.FindProperty("hintLabel").objectReferenceValue = hint;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            contentRoot.SetActive(false);

            EnsureEventSystem();
            bool pauseButtonAdded = AddPauseButton(controls);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = canvasObject;
            Debug.Log("Muki: pantalla 'Cómo se juega' creada en EscenaPrueba" +
                      (pauseButtonAdded ? " y botón añadido al menú de pausa." : ". No se encontró el menú de pausa: ejecuta antes 'Build Pause Menu UI' y vuelve a ejecutar esto."));
        }

        private static void BuildRow(Transform parent, Row row, float yTop, float yBottom, TMP_FontAsset bodyFont, TMP_FontAsset boldFont)
        {
            GameObject rowObject = Rect("Fila_" + row.key.Replace(" ", ""), parent, new Vector2(0f, yBottom), new Vector2(1f, yTop));

            // Tecla (se centra verticalmente en la primera línea de la fila)
            float keyTop = row.detail == null ? .88f : .94f;
            float keyBottom = row.detail == null ? .12f : .56f;
            if (row.detail != null) { keyTop = .90f; keyBottom = .54f; }
            GameObject keycap = Rect("Tecla", rowObject.transform, new Vector2(0f, keyBottom), new Vector2(.27f, keyTop));
            keycap.AddComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, .55f);
            Outline outline = keycap.AddComponent<Outline>();
            outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, .85f);
            outline.effectDistance = new Vector2(2f, 2f);
            Text(keycap.transform, row.key, boldFont, 28f, Gold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, 3f);

            // Descripción
            if (row.detail == null)
            {
                Text(rowObject.transform, row.title, bodyFont, 32f, Cream, TextAlignmentOptions.Left, new Vector2(.31f, .12f), new Vector2(1f, .88f), 0f);
            }
            else
            {
                Text(rowObject.transform, row.title, bodyFont, 32f, Cream, TextAlignmentOptions.Left, new Vector2(.31f, .54f), new Vector2(1f, .90f), 0f);
                TextMeshProUGUI detail = Text(rowObject.transform, row.detail, bodyFont, 22f, new Color(Cream.r, Cream.g, Cream.b, .78f), TextAlignmentOptions.TopLeft, new Vector2(.31f, .02f), new Vector2(1f, .54f), 0f);
                detail.fontStyle = FontStyles.Italic;
            }
        }

        private static Button CloseButton(Transform parent, TMP_FontAsset font, out TMP_Text label)
        {
            GameObject buttonObject = new GameObject("Cerrar_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(MenuButtonFeedback));
            RectTransform rect = buttonObject.transform as RectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(.30f, .095f); rect.anchorMax = new Vector2(.70f, .175f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, .35f);
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = Brown; outline.effectDistance = new Vector2(2.5f, 2.5f); outline.useGraphicAlpha = false;
            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, .9f, .65f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(.85f, .65f, .35f, 1f);
            button.colors = colors;
            label = Text(rect, "COMENZAR", font, 30f, Gold, TextAlignmentOptions.Center, new Vector2(.04f, .1f), new Vector2(.96f, .9f), 3f);
            return button;
        }

        /// <summary>Añade "CÓMO SE JUEGA" al menú de pausa clonando el estilo de sus botones y reacomoda los 4.</summary>
        private static bool AddPauseButton(ControlsPanel controls)
        {
            PauseMenuController pause = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
            if (pause == null) return false;

            Transform buttons = pause.transform.Find("PausePanel/Panel_Pausa/Botones");
            if (buttons == null) return false;

            Transform old = buttons.Find("ComoSeJuega_Button");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            Transform resume = buttons.Find("Reanudar_Button");
            Transform menu = buttons.Find("MenuPrincipal_Button");
            Transform quit = buttons.Find("Salir_Button");
            if (resume == null || menu == null || quit == null) return false;

            GameObject clone = Object.Instantiate(menu.gameObject, buttons);
            clone.name = "ComoSeJuega_Button";

            Button cloneButton = clone.GetComponent<Button>();
            for (int i = cloneButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(cloneButton.onClick, i);
            UnityEventTools.AddPersistentListener(cloneButton.onClick, controls.OpenFromPause);

            TMP_Text label = clone.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = "CÓMO SE JUEGA";
                label.gameObject.name = "Texto_COMO_SE_JUEGA";
            }

            SetAnchors(resume, .0f, .78f, 1f, 1f);
            SetAnchors(clone.transform, .0f, .52f, 1f, .72f);
            SetAnchors(menu, .0f, .26f, 1f, .46f);
            SetAnchors(quit, .0f, .0f, 1f, .20f);
            return true;
        }

        private static void SetAnchors(Transform t, float xMin, float yMin, float xMax, float yMax)
        {
            RectTransform rect = (RectTransform)t;
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
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
            GameObject go = Rect("Texto_" + value.Replace(" ", "_"), parent, min, max);
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
