using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Muki.UI.Editor
{
    public static class ThoughtDialogUIBuilder
    {
        private const string FontPath = "Assets/UI/Tipografia/Subtitulos/CormorantGaramond-SemiBold SDF.asset";

        // Paleta del juego (misma que el resto de builders de UI)
        private static readonly Color Ink = Hex("#000000");
        private static readonly Color Gold = Hex("#ecbb48");
        private static readonly Color Brown = Hex("#71491f");
        private static readonly Color Cream = Hex("#F0D9A2");

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        // Una entrada por noticia (índice 0 = Noticia1 ... 4 = Noticia final). Cada string es una "página".
        private static readonly string[][] NewsThoughts =
        {
            new[]
            {
                "¿Dieciséis mineros desaparecidos...? No puede ser.",
                "¿Qué demonios está pasando ahí abajo?",
                "Si sellaron la entrada, debe ser porque encontraron algo... o porque tienen miedo de que alguien más entre."
            },
            new[]
            {
                "Dicen que están revisando los planos y buscando fallas geológicas...",
                "Espero que encuentren algo pronto. Dieciséis hombres no pueden desaparecer así como así.",
                "Alguien tiene que saber qué ocurrió realmente."
            },
            new[]
            {
                "¿El Muki? Mi abuelo me contaba esas historias cuando era niño.",
                "Siempre pensé que eran cuentos para asustar a los nuevos...",
                "Pero si no hubo ningún derrumbe, ¿entonces qué fue lo que pasó allá abajo?"
            },
            new[]
            {
                "Ya ni siquiera hablan del tema. Dieciséis hombres siguen desaparecidos y parece que a nadie le importa.",
                "¿De verdad van a dejar que todo quede así?",
                "No sé qué esconden, pero no pienso olvidarme de esto."
            },
            new[]
            {
                "¿Cómo que la entrada está abierta? ¡Si la sellaron con explosivos!",
                "Esto no tiene ningún sentido... Tengo que averiguar qué está pasando.",
                "Quizás todavía haya respuestas ahí dentro, aunque no me guste lo que pueda encontrar."
            }
        };

        [MenuItem("Muki/UI/Crear cuadro de pensamientos")]
        public static void Build()
        {
            if (Object.FindFirstObjectByType<ThoughtDialogUI>() != null)
            {
                Debug.LogWarning("Ya existe un ThoughtDialogUI en la escena.");
                return;
            }

            GameObject canvasGo = new GameObject("ThoughtCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Marco exterior marrón de la paleta
            GameObject panel = new GameObject("ThoughtPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(ThoughtDialogUI));
            panel.transform.SetParent(canvasGo.transform, false);
            RectTransform pr = (RectTransform)panel.transform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0f);
            pr.anchoredPosition = new Vector2(0f, 50f);
            pr.sizeDelta = new Vector2(1000f, 190f);
            panel.GetComponent<Image>().color = Brown;
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // Fondo negro interior
            GameObject inner = new GameObject("Background", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(panel.transform, false);
            RectTransform ir = (RectTransform)inner.transform;
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(6f, 6f); ir.offsetMax = new Vector2(-6f, -6f);
            inner.GetComponent<Image>().color = new Color(Ink.r, Ink.g, Ink.b, .9f);

            // Línea dorada superior, igual que el panel del cofre
            GameObject accent = new GameObject("Acento_Dorado", typeof(RectTransform), typeof(Image));
            accent.transform.SetParent(inner.transform, false);
            RectTransform ar = (RectTransform)accent.transform;
            ar.anchorMin = new Vector2(.02f, 1f); ar.anchorMax = new Vector2(.98f, 1f);
            ar.pivot = new Vector2(.5f, 1f);
            ar.anchoredPosition = new Vector2(0f, -6f);
            ar.sizeDelta = new Vector2(0f, 3f);
            accent.GetComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, .7f);

            // Espacio para la expresión del jugador (izquierda)
            GameObject frame = new GameObject("PortraitFrame", typeof(RectTransform), typeof(Image), typeof(Outline));
            frame.transform.SetParent(inner.transform, false);
            RectTransform fr = (RectTransform)frame.transform;
            fr.anchorMin = fr.anchorMax = new Vector2(0f, 0.5f);
            fr.pivot = new Vector2(0f, 0.5f);
            fr.anchoredPosition = new Vector2(20f, -4f);
            fr.sizeDelta = new Vector2(140f, 140f);
            frame.GetComponent<Image>().color = new Color(Brown.r, Brown.g, Brown.b, .35f);
            Outline frameOutline = frame.GetComponent<Outline>();
            frameOutline.effectColor = new Color(Gold.r, Gold.g, Gold.b, .85f);
            frameOutline.effectDistance = new Vector2(2f, -2f);

            GameObject portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portrait.transform.SetParent(frame.transform, false);
            RectTransform por = (RectTransform)portrait.transform;
            por.anchorMin = Vector2.zero; por.anchorMax = Vector2.one;
            por.offsetMin = new Vector2(4f, 4f); por.offsetMax = new Vector2(-4f, -4f);
            Image portraitImage = portrait.GetComponent<Image>();
            portraitImage.preserveAspect = true;
            portraitImage.color = Color.white;
            portraitImage.enabled = false;

            // Texto
            GameObject textGo = new GameObject("ThoughtText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(inner.transform, false);
            RectTransform tr = (RectTransform)textGo.transform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(185f, 15f); tr.offsetMax = new Vector2(-25f, -15f);
            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font != null) tmp.font = font;
            tmp.fontSize = 34f;
            tmp.color = Cream;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.text = string.Empty;

            SerializedObject so = new SerializedObject(panel.GetComponent<ThoughtDialogUI>());
            so.FindProperty("group").objectReferenceValue = cg;
            so.FindProperty("portraitImage").objectReferenceValue = portraitImage;
            so.FindProperty("thoughtText").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(canvasGo, "Crear cuadro de pensamientos");
            Selection.activeGameObject = panel;
            EditorGUIUtility.PingObject(panel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasGo.scene);
            Debug.Log("ThoughtCanvas creado en la escena '" + canvasGo.scene.name + "'. Guarda la escena (Ctrl+S).");
        }

        /// <summary>
        /// PlayerInteractor solo detecta objetos en su interactLayer. Las radios que estén en otra
        /// capa (por ejemplo Default) no responden a la tecla E, así que se mueven a una capa válida.
        /// </summary>
        [MenuItem("Muki/UI/Corregir capa de las radios")]
        public static void FixRadioLayers()
        {
            var interactor = Object.FindFirstObjectByType<StarterAssets.PlayerInteractor>();
            if (interactor == null)
            {
                Debug.LogWarning("No hay PlayerInteractor en la escena.");
                return;
            }

            int mask = interactor.interactLayer.value;
            if (mask == 0)
            {
                Debug.LogWarning("PlayerInteractor no tiene interactLayer configurado.");
                return;
            }

            int targetLayer = -1;
            for (int i = 0; i < 32 && targetLayer < 0; i++)
                if ((mask & (1 << i)) != 0) targetLayer = i;

            int fixedCount = 0;
            foreach (var radio in Object.FindObjectsByType<StarterAssets.RadioInteractable>(FindObjectsSortMode.None))
            {
                foreach (Collider col in radio.GetComponentsInChildren<Collider>(true))
                {
                    if ((mask & (1 << col.gameObject.layer)) != 0) continue;
                    Undo.RecordObject(col.gameObject, "Corregir capa de radio");
                    col.gameObject.layer = targetLayer;
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"Capa de interacción corregida en {fixedCount} collider(s) de radios (capa {LayerMask.LayerToName(targetLayer)}).");
        }

        /// <summary>
        /// Pone en cada radio la expresión del minero de su noticia (Noticia1..5 → Minero_Noticia1..5).
        /// Solo toca el sprite de cada pensamiento: los textos quedan como estén.
        /// </summary>
        [MenuItem("Muki/UI/Asignar expresiones del minero")]
        public static void AssignExpressions()
        {
            int assigned = 0;

            foreach (var radio in Object.FindObjectsByType<StarterAssets.RadioInteractable>(FindObjectsSortMode.None))
            {
                if (radio.radioAudio == null) continue;

                Match m = Regex.Match(radio.radioAudio.name, @"Noticia\s*(\d+)", RegexOptions.IgnoreCase);
                if (!m.Success) continue;
                int news = int.Parse(m.Groups[1].Value);

                var trigger = radio.GetComponent<RadioThoughtTrigger>();
                if (trigger == null) continue;

                string path = $"Assets/UI/Expresiones/Minero_Noticia{news}.png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogWarning("Muki: no se encontró " + path);
                    continue;
                }

                SerializedObject so = new SerializedObject(trigger);
                SerializedProperty list = so.FindProperty("thoughts");
                for (int i = 0; i < list.arraySize; i++)
                    list.GetArrayElementAtIndex(i).FindPropertyRelative("expression").objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                assigned++;
            }

            if (assigned > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"Expresiones asignadas a {assigned} radio(s).");
        }

        [MenuItem("Muki/UI/Asignar pensamientos a las radios")]
        public static void AssignToRadios()
        {
            // Sin cuadro en la escena los pensamientos no tienen dónde mostrarse: se crea aquí también.
            if (Object.FindFirstObjectByType<ThoughtDialogUI>(FindObjectsInactive.Include) == null)
                Build();

            FixRadioLayers();

            var radios = Object.FindObjectsByType<StarterAssets.RadioInteractable>(FindObjectsSortMode.None);
            int assigned = 0;

            foreach (var radio in radios)
            {
                if (radio.radioAudio == null) continue;

                // "Noticia1" .. "Noticia5": el número del audio decide qué pensamientos usa.
                Match m = Regex.Match(radio.radioAudio.name, @"Noticia\s*(\d+)", RegexOptions.IgnoreCase);
                if (!m.Success) continue;
                int index = int.Parse(m.Groups[1].Value) - 1;
                if (index < 0 || index >= NewsThoughts.Length) continue;

                var trigger = radio.GetComponent<RadioThoughtTrigger>();
                if (trigger == null) trigger = Undo.AddComponent<RadioThoughtTrigger>(radio.gameObject);

                SerializedObject so = new SerializedObject(trigger);
                SerializedProperty list = so.FindProperty("thoughts");
                string[] pages = NewsThoughts[index];
                list.arraySize = pages.Length;
                for (int i = 0; i < pages.Length; i++)
                {
                    SerializedProperty item = list.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("text").stringValue = pages[i];
                }
                so.ApplyModifiedProperties();
                assigned++;
            }

            if (assigned > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"Pensamientos asignados a {assigned} radio(s).");
        }
    }
}
