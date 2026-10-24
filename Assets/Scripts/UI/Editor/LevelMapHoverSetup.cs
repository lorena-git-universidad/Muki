using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class LevelMapHoverSetup
    {
        private const string ScenePath = "Assets/Scenes/SeleccionarNivel.unity";
        private const string MapsFolder = "Assets/UI/Mapas/";
        private const string CaptionFontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-Medium SDF.asset";
        private const string CaptionMaterialPath = "Assets/UI/Mapas/Texto_Mapa_Material.mat";

        [MenuItem("Muki/Setup Level Select Map Hover")]
        public static void Setup()
        {
            AssetDatabase.Refresh();

            Texture normal = LoadMap("Mapa_Normal.png");
            Texture muki = LoadMap("Mapa_Muki.png");
            Texture llorona = LoadMap("Mapa_Llorona.png");
            Texture curupira = LoadMap("Mapa_Curupira.png");
            if (normal == null || muki == null || llorona == null || curupira == null) return;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject mapObject = GameObject.Find("Mapa_Imagen");
            GameObject mukiButton = GameObject.Find("Muki_Button");
            GameObject lloronaButton = GameObject.Find("Proximamente_Button");      // el de arriba
            GameObject curupiraButton = GameObject.Find("Proximamente_Button (1)"); // el de abajo

            if (mapObject == null || mukiButton == null || lloronaButton == null || curupiraButton == null)
            {
                Debug.LogError("Muki: faltan objetos en SeleccionarNivel (Mapa_Imagen, Muki_Button, Proximamente_Button, Proximamente_Button (1)).");
                return;
            }

            RawImage front = mapObject.GetComponent<RawImage>();
            front.raycastTarget = false;

            // Segunda imagen idéntica debajo, para el fundido cruzado entre mapas.
            Transform oldBack = mapObject.transform.parent.Find("Mapa_Imagen_Anterior");
            if (oldBack != null) Object.DestroyImmediate(oldBack.gameObject);

            GameObject backObject = Object.Instantiate(mapObject, mapObject.transform.parent);
            backObject.name = "Mapa_Imagen_Anterior";
            backObject.transform.SetSiblingIndex(mapObject.transform.GetSiblingIndex());
            RawImage back = backObject.GetComponent<RawImage>();
            back.raycastTarget = false;

            LevelMapHover hover = mapObject.GetComponent<LevelMapHover>();
            if (hover == null) hover = mapObject.AddComponent<LevelMapHover>();

            SerializedObject serializedHover = new SerializedObject(hover);
            serializedHover.FindProperty("front").objectReferenceValue = front;
            serializedHover.FindProperty("back").objectReferenceValue = back;
            serializedHover.FindProperty("normalMap").objectReferenceValue = normal;
            serializedHover.ApplyModifiedPropertiesWithoutUndo();

            front.texture = normal;
            back.texture = normal;

            // La copia no necesita (ni debe tener) el controlador del mapa principal.
            LevelMapHover clonedHover = backObject.GetComponent<LevelMapHover>();
            if (clonedHover != null) Object.DestroyImmediate(clonedHover);

            Transform captionsRoot = EnsureCaptionsRoot(mapObject.transform.parent);
            CanvasGroup[] mukiTexts = EnsureCaptions(captionsRoot, "Muki", new[] { "En las cuevas suele estar", "En las minas suele habitar" }, new Vector2(930f, 130f));
            CanvasGroup[] lloronaTexts = EnsureCaptions(captionsRoot, "Llorona", new[] { "Su llanto anuncia su llegada", "Busca entre las sombras lo que perdió" }, new Vector2(1130f, 380f));
            CanvasGroup[] curupiraTexts = EnsureCaptions(captionsRoot, "Curupira", new[] { "A los intrusos suele engañar", "Confunde a quien invade su hogar" }, new Vector2(1480f, -230f));

            Link(mukiButton, hover, muki, mukiTexts);
            Link(lloronaButton, hover, llorona, lloronaTexts);
            Link(curupiraButton, hover, curupira, curupiraTexts);

            WireIntroForLastButton(curupiraButton);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = mapObject;
            Debug.Log("Muki: el mapa de SeleccionarNivel ahora cambia al pasar el cursor por cada botón.");
        }

        /// <summary>El último botón "Próximamente" también debe entrar con la animación de la escena.</summary>
        private static void WireIntroForLastButton(GameObject lastButton)
        {
            PrototypeSceneIntroAnimation intro = Object.FindFirstObjectByType<PrototypeSceneIntroAnimation>();
            if (intro == null)
            {
                Debug.LogWarning("Muki: no hay PrototypeSceneIntroAnimation en la escena.");
                return;
            }

            CanvasGroup group = lastButton.GetComponent<CanvasGroup>();
            if (group == null) group = lastButton.AddComponent<CanvasGroup>();

            SerializedObject serialized = new SerializedObject(intro);
            serialized.FindProperty("upcomingButton2").objectReferenceValue = group;
            serialized.FindProperty("upcomingRect2").objectReferenceValue = lastButton.transform as RectTransform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Link(GameObject button, LevelMapHover hover, Texture highlighted, CanvasGroup[] captions)
        {
            LevelMapHoverTarget target = button.GetComponent<LevelMapHoverTarget>();
            if (target == null) target = button.AddComponent<LevelMapHoverTarget>();

            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty("map").objectReferenceValue = hover;
            serialized.FindProperty("highlightedMap").objectReferenceValue = highlighted;

            SerializedProperty list = serialized.FindProperty("captions");
            list.arraySize = captions.Length;
            for (int i = 0; i < captions.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = captions[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------------------------------
        // Textos que aparecen junto a la zona del mapa. Si ya existen NO se tocan (así se conservan
        // las posiciones que se hayan ajustado a mano); solo se crean los que falten.
        // ---------------------------------------------------------------------------------------

        private static Transform EnsureCaptionsRoot(Transform panel)
        {
            Transform root = panel.Find("Textos_Mapa");
            if (root != null) return root;

            GameObject go = new GameObject("Textos_Mapa", typeof(RectTransform));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(panel, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static CanvasGroup[] EnsureCaptions(Transform root, string legend, string[] lines, Vector2 firstPosition)
        {
            Material material = EnsureCaptionMaterial();
            TMPro.TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(CaptionFontPath);
            CanvasGroup[] result = new CanvasGroup[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string objectName = $"Texto_{legend}_{i + 1}";
                Transform existing = root.Find(objectName);
                GameObject go;

                if (existing != null)
                {
                    go = existing.gameObject;
                }
                else
                {
                    go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasGroup), typeof(TMPro.TextMeshProUGUI));
                    RectTransform rect = (RectTransform)go.transform;
                    rect.SetParent(root, false);
                    rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                    rect.pivot = new Vector2(.5f, .5f);
                    rect.sizeDelta = new Vector2(420f, 120f);
                    rect.anchoredPosition = firstPosition + Vector2.down * 120f * i;
                    rect.localScale = Vector3.one;

                    TMPro.TextMeshProUGUI text = go.GetComponent<TMPro.TextMeshProUGUI>();
                    text.text = lines[i];
                    if (font != null) text.font = font;
                    if (material != null) text.fontSharedMaterial = material;
                    text.fontSize = 44f;
                    text.color = Hex("#ecbb48");
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.textWrappingMode = TMPro.TextWrappingModes.Normal;
                    text.raycastTarget = false;
                }

                CanvasGroup group = go.GetComponent<CanvasGroup>();
                if (group == null) group = go.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
                result[i] = group;
            }

            return result;
        }

        /// <summary>Material del texto con el borde marrón de la paleta (se guarda como asset para que persista).</summary>
        private static Material EnsureCaptionMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(CaptionMaterialPath);
            if (existing != null) return existing;

            TMPro.TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(CaptionFontPath);
            if (font == null) return null;

            Material material = new Material(font.material) { name = "Texto_Mapa_Material" };
            material.EnableKeyword("OUTLINE_ON");
            material.SetFloat(TMPro.ShaderUtilities.ID_OutlineWidth, 0.2f);
            material.SetColor(TMPro.ShaderUtilities.ID_OutlineColor, Hex("#71491f"));
            AssetDatabase.CreateAsset(material, CaptionMaterialPath);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        private static Texture LoadMap(string fileName)
        {
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(MapsFolder + fileName);
            if (texture == null) Debug.LogError("Muki: no se encontró " + MapsFolder + fileName);
            return texture;
        }
    }
}
