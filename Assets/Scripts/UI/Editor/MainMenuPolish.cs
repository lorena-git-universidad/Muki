using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class MainMenuPolish
    {
        private static readonly Color Black = Hex("000000");
        private static readonly Color Gold = Hex("ecbb48");
        private static readonly Color Charcoal = Hex("575352");
        private static readonly Color Terracotta = Hex("d57f40");
        private static readonly Color Brown = Hex("71491f");
        private static readonly Color Red = Hex("ac2711");

        [MenuItem("Muki/Polish Main Menu")]
        public static void Polish()
        {
            const string scenePath = "Assets/Scenes/MenuPrincipal.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Muki: no se encontró MenuCanvas en MenuPrincipal.");
                return;
            }

            Transform title = Find("Title");
            Transform titleBlock = Find("TitleBlock");
            Transform menuPanel = Find("MenuPanel");
            Transform play = Find("PlayButton");
            Transform quit = Find("QuitButton");

            if (title != null)
            {
                TMP_Text titleText = title.GetComponent<TMP_Text>();
                if (titleText != null)
                {
                    titleText.fontSize = 86;
                    titleText.characterSpacing = 7f;
                    titleText.lineSpacing = -8f;
                    titleText.color = Gold;
                }
            }

            if (titleBlock != null)
            {
                RectTransform rect = titleBlock as RectTransform;
                rect.anchorMin = new Vector2(0.085f, 0.32f);
                rect.anchorMax = new Vector2(0.54f, 0.72f);
            }

            if (menuPanel != null)
            {
                RectTransform rect = menuPanel as RectTransform;
                rect.anchorMin = new Vector2(0.64f, 0.28f);
                rect.anchorMax = new Vector2(0.91f, 0.72f);
            }

            StyleButton(play, new Color(0.443f, 0.286f, 0.122f, 0.48f), Gold, Terracotta, 2.5f);
            StyleButton(quit, new Color(0.341f, 0.325f, 0.322f, 0.14f), new Color(0.78f, 0.75f, 0.69f), Brown, 1.5f);

            Transform menuLabel = Find("MenuLabel");
            if (menuLabel != null)
            {
                TMP_Text label = menuLabel.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.text = "ELIGE TU CAMINO";
                    label.fontSize = 21;
                    label.characterSpacing = 3f;
                    label.color = new Color(0.78f, 0.75f, 0.69f, 0.82f);
                }
            }

            CreateOrUpdateText(canvas.transform, "UX_MenuEyebrow", "CAPÍTULO I  /  EL LLAMADO", 17, Charcoal,
                new Vector2(0.64f, 0.755f), new Vector2(0.91f, 0.79f), TextAlignmentOptions.Left);
            CreateOrUpdateText(canvas.transform, "UX_InputHint", "ENTER  ·  CONTINUAR       ESC  ·  SALIR", 16, new Color(0.78f, 0.75f, 0.69f, 0.58f),
                new Vector2(0.64f, 0.085f), new Vector2(0.93f, 0.115f), TextAlignmentOptions.Left);
            CreateOrUpdateText(canvas.transform, "UX_Signature", "MUKI  /  01", 16, Charcoal,
                new Vector2(0.085f, 0.07f), new Vector2(0.34f, 0.1f), TextAlignmentOptions.Left);

            CreateOrUpdateRule(canvas.transform, "UX_CenterRail", new Color(Terracotta.r, Terracotta.g, Terracotta.b, 0.7f),
                new Vector2(0.605f, 0.13f), new Vector2(0.607f, 0.87f));
            CreateOrUpdateRule(canvas.transform, "UX_MenuHairline", new Color(Gold.r, Gold.g, Gold.b, 0.42f),
                new Vector2(0.64f, 0.735f), new Vector2(0.91f, 0.737f));

            AddAccentBar(play, "UX_PlayAccent", Terracotta);
            EnsureKeyboardController();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Muki: MenuPrincipal mejorado y guardado.");
        }

        private static void StyleButton(Transform target, Color fill, Color textColor, Color borderColor, float borderSize)
        {
            if (target == null) return;
            Image image = target.GetComponent<Image>();
            if (image != null) image.color = fill;
            Outline outline = target.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = borderColor;
                outline.effectDistance = new Vector2(borderSize, borderSize);
            }

            TMP_Text text = target.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.color = textColor;
                text.fontSize = 39;
                text.characterSpacing = 2f;
            }

            MenuButtonFeedback feedback = target.GetComponent<MenuButtonFeedback>();
            if (feedback == null) target.gameObject.AddComponent<MenuButtonFeedback>();
        }

        private static void AddAccentBar(Transform target, string name, Color color)
        {
            if (target == null || Find(name) != null) return;
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.transform as RectTransform;
            rect.SetParent(target, false);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.012f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
        }

        private static TMP_Text CreateOrUpdateText(Transform parent, string name, string value, float size, Color color, Vector2 min, Vector2 max, TextAlignmentOptions alignment)
        {
            Transform existing = Find(name);
            TMP_Text text = existing != null ? existing.GetComponent<TMP_Text>() : null;
            if (text == null)
            {
                GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
                text = go.GetComponent<TextMeshProUGUI>();
                text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Regular SDF.asset");
            }

            RectTransform rect = text.transform as RectTransform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment; text.raycastTarget = false; text.enableWordWrapping = false;
            return text;
        }

        private static Image CreateOrUpdateRule(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            Transform existing = Find(name);
            Image image = existing != null ? existing.GetComponent<Image>() : null;
            if (image == null)
            {
                GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                image = go.GetComponent<Image>(); image.raycastTarget = false;
            }
            RectTransform rect = image.transform as RectTransform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            image.color = color;
            image.transform.SetAsFirstSibling();
            return image;
        }

        private static void EnsureKeyboardController()
        {
            MainMenuController controller = Object.FindFirstObjectByType<MainMenuController>();
            if (controller == null) return;
            SerializedObject serialized = new SerializedObject(controller);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform Find(string name)
        {
            foreach (GameObject rootObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform result = FindRecursive(rootObject.transform, name);
                if (result != null) return result;
            }
            return null;
        }

        private static Transform FindRecursive(Transform current, string name)
        {
            if (current.name == name) return current;
            foreach (Transform child in current) { Transform result = FindRecursive(child, name); if (result != null) return result; }
            return null;
        }

        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out Color color); return color; }
    }
}
