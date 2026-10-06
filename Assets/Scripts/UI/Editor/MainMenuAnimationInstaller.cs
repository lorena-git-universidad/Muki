using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Muki.UI;

namespace Muki.Editor
{
    public static class MainMenuAnimationInstaller
    {
        [MenuItem("Muki/Install Main Menu Animations")]
        public static void Install()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity", OpenSceneMode.Single);
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Muki: no se encontró MenuCanvas.");
                return;
            }

            MainMenuIntroAnimation animation = canvas.GetComponent<MainMenuIntroAnimation>();
            if (animation == null) animation = canvas.gameObject.AddComponent<MainMenuIntroAnimation>();

            CreateOrFindOverlay(canvas.transform);
            WireButtonEvents();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Muki: animaciones de entrada instaladas en MenuCanvas.");
        }

        private static void WireButtonEvents()
        {
            MainMenuController controller = Object.FindFirstObjectByType<MainMenuController>();
            if (controller == null) return;

            Button play = FindButton("PlayButton");
            Button quit = FindButton("QuitButton");
            if (play != null)
            {
                ClearPersistentListeners(play.onClick);
                UnityEventTools.AddPersistentListener(play.onClick, controller.Play);
            }
            if (quit != null)
            {
                ClearPersistentListeners(quit.onClick);
                UnityEventTools.AddPersistentListener(quit.onClick, controller.Quit);
            }
        }

        private static void ClearPersistentListeners(UnityEngine.Events.UnityEventBase events)
        {
            while (events.GetPersistentEventCount() > 0)
                UnityEventTools.RemovePersistentListener(events, 0);
        }

        private static Button FindButton(string objectName)
        {
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button.gameObject.name == objectName) return button;
                foreach (TMPro.TMP_Text label in button.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    if (label.text.Trim().Equals(objectName == "PlayButton" ? "JUGAR" : "SALIR", System.StringComparison.OrdinalIgnoreCase)) return button;
            }
            return null;
        }

        private static void CreateOrFindOverlay(Transform canvas)
        {
            Transform existing = canvas.Find("UX_TransitionOverlay");
            if (existing != null) return;
            GameObject go = new GameObject("UX_TransitionOverlay", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(CanvasGroup));
            RectTransform rect = go.transform as RectTransform;
            rect.SetParent(canvas, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            UnityEngine.UI.Image image = go.GetComponent<UnityEngine.UI.Image>(); image.color = Color.black; image.raycastTarget = false;
            go.transform.SetAsLastSibling();
        }
    }
}
