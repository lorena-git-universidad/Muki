using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    /// <summary>Crea en EscenaPrueba el HUD de vidas (3 corazones arriba a la izquierda).</summary>
    public static class LivesHUDBuilder
    {
        private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
        private const string FullHeartPath = "Assets/UI/Vidas/Corazon_Lleno.png";
        private const string LostHeartPath = "Assets/UI/Vidas/Corazon_Perdido.png";
        private const int HeartCount = 3;

        [MenuItem("Muki/Build Lives HUD")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            AssetDatabase.ImportAsset(FullHeartPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(LostHeartPath, ImportAssetOptions.ForceSynchronousImport);
            Sprite full = AssetDatabase.LoadAssetAtPath<Sprite>(FullHeartPath);
            Sprite lost = AssetDatabase.LoadAssetAtPath<Sprite>(LostHeartPath);
            if (full == null || lost == null)
            {
                Debug.LogError("Muki: no se encontraron los sprites de los corazones en Assets/UI/Vidas/.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "LivesCanvas") Object.DestroyImmediate(root);

            GameObject canvasObject = new GameObject("LivesCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(LivesHUD));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30; // sobre el juego, bajo la pausa (100) y los controles (110)
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            // Contenedor arriba a la izquierda; los corazones se acomodan solos en fila.
            GameObject container = new GameObject("Vidas", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform containerRect = (RectTransform)container.transform;
            containerRect.SetParent(canvasObject.transform, false);
            containerRect.anchorMin = containerRect.anchorMax = containerRect.pivot = new Vector2(0f, 1f);
            containerRect.anchoredPosition = new Vector2(40f, -40f);
            containerRect.sizeDelta = new Vector2(HeartCount * 80f + (HeartCount - 1) * 10f, 90f);

            HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Image[] hearts = new Image[HeartCount];
            for (int i = 0; i < HeartCount; i++)
            {
                GameObject heart = new GameObject("Corazon_" + (i + 1), typeof(RectTransform), typeof(Image));
                RectTransform rect = (RectTransform)heart.transform;
                rect.SetParent(container.transform, false);
                rect.sizeDelta = new Vector2(80f, 90f);

                Image image = heart.GetComponent<Image>();
                image.sprite = full;
                image.preserveAspect = true;
                image.raycastTarget = false;
                hearts[i] = image;
            }

            SerializedObject serialized = new SerializedObject(canvasObject.GetComponent<LivesHUD>());
            SerializedProperty list = serialized.FindProperty("hearts");
            list.arraySize = HeartCount;
            for (int i = 0; i < HeartCount; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            serialized.FindProperty("fullHeart").objectReferenceValue = full;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = canvasObject;
            Debug.Log("Muki: HUD de vidas creado en EscenaPrueba (3 corazones arriba a la izquierda).");
        }
    }
}
