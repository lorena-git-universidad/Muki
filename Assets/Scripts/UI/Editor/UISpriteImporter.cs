using UnityEditor;
using UnityEngine;

namespace Muki.Editor
{
    /// <summary>
    /// Importa como Sprite (UI) las imágenes de expresiones del minero y de los corazones,
    /// para que se puedan usar directamente en Image sin configurar cada PNG a mano.
    /// </summary>
    public sealed class UISpriteImporter : AssetPostprocessor
    {
        private static readonly string[] SpriteFolders =
        {
            "Assets/UI/Expresiones/",
            "Assets/UI/Vidas/",
        };

        private void OnPreprocessTexture()
        {
            foreach (string folder in SpriteFolders)
            {
                if (!assetPath.StartsWith(folder)) continue;

                TextureImporter importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 1024;
                return;
            }
        }
    }
}
