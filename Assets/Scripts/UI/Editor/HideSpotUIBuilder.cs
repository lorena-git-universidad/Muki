using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

namespace Muki.Editor
{
    /// <summary>
    /// Copies the existing chest world-space canvas to every HideSpot. This
    /// builder changes presentation only; HideSpot and PlayerHide keep all
    /// interaction behaviour they already have.
    /// </summary>
    public static class HideSpotUIBuilder
    {
        private const string ScenePath = "Assets/Scenes/EscenaPrueba.unity";
        private const string ChestCanvasName = "CofreUI_Visual";
        private const string WagonCanvasName = "VagonUI_Visual";
        private const string ActionText = "[E]  PARA ESCONDERTE";

        [MenuItem("Muki/Copy Chest Canvas To Hide Spots")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject chestCanvas = GameObject.Find(ChestCanvasName);
            if (chestCanvas == null)
            {
                Debug.LogError($"No se encontró '{ChestCanvasName}' en la escena.");
                return;
            }

            HideSpot[] hideSpots = Object.FindObjectsByType<HideSpot>(FindObjectsSortMode.None);
            int copied = 0;
            foreach (HideSpot hideSpot in hideSpots)
            {
                if (hideSpot == null) continue;

                Transform oldCanvas = hideSpot.transform.Find(WagonCanvasName);
                if (oldCanvas != null) Object.DestroyImmediate(oldCanvas.gameObject);

                GameObject wagonCanvas = Object.Instantiate(chestCanvas, hideSpot.transform);
                wagonCanvas.name = WagonCanvasName;
                Undo.RegisterCreatedObjectUndo(wagonCanvas, "Copiar canvas del cofre a las vagonetas");

                // Keep the same screen/world size as the chest canvas.
                Vector3 chestWorldScale = chestCanvas.transform.lossyScale;
                Vector3 parentWorldScale = hideSpot.transform.lossyScale;
                wagonCanvas.transform.localScale = new Vector3(
                    SafeDivide(chestWorldScale.x, parentWorldScale.x),
                    SafeDivide(chestWorldScale.y, parentWorldScale.y),
                    SafeDivide(chestWorldScale.z, parentWorldScale.z));
                wagonCanvas.transform.localPosition = new Vector3(0f, 2.1f, 0f);
                wagonCanvas.transform.localRotation = Quaternion.identity;

                ChestUIVisual chestLogic = wagonCanvas.GetComponent<ChestUIVisual>();
                if (chestLogic != null) Object.DestroyImmediate(chestLogic);

                TMP_Text actionText = FindActionText(wagonCanvas.transform);
                if (actionText != null)
                {
                    actionText.text = ActionText;
                    actionText.name = "Texto_Esconderte";
                }
                else
                {
                    Debug.LogWarning($"No se encontró el texto de acción en '{wagonCanvas.name}'.");
                }

                DisableOldHidePrompt(hideSpot.transform);
                copied++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = hideSpots.Length > 0 ? hideSpots[0].gameObject : chestCanvas;
            Debug.Log($"Muki: canvas del cofre copiado a {copied} HideSpot con el texto '{ActionText}'.");
        }

        private static TMP_Text FindActionText(Transform root)
        {
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text.text.Contains("DEPOSITAR")) return text;
            }

            return texts.Length > 0 ? texts[texts.Length - 1] : null;
        }

        private static void DisableOldHidePrompt(Transform hideSpot)
        {
            TMP_Text[] prompts = hideSpot.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text prompt in prompts)
            {
                if (prompt.transform.IsChildOf(hideSpot) && prompt.text.Contains("Hide"))
                    prompt.gameObject.SetActive(false);
            }
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) < 0.0001f ? value : value / divisor;
        }
    }
}
