using System.Collections.Generic;
using UnityEditor;

namespace Muki.Editor
{
    public static class BuildSettingsRepair
    {
        [MenuItem("Muki/Add Existing Scenes To Build Settings")]
        public static void AddExistingScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            AddIfMissing(scenes, "Assets/Scenes/MenuPrincipal.unity");
            AddIfMissing(scenes, "Assets/Scenes/SeleccionarNivel.unity");
            AddIfMissing(scenes, "Assets/Scenes/EscenaPrueba.unity");
            AddIfMissing(scenes, "Assets/Scenes/EscenaJuego.unity");
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("Muki: EscenaPrueba agregada correctamente a Build Settings.");
        }

        private static void AddIfMissing(List<EditorBuildSettingsScene> scenes, string path)
        {
            if (System.IO.File.Exists(path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }
}
