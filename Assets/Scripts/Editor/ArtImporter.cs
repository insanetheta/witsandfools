using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public sealed class ArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;

            if (assetPath.Contains("/Textures/"))
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 512;
            }
            else if (assetPath.Contains("/Backgrounds/") || assetPath.Contains("/Tables/") || assetPath.Contains("/Scenes/"))
                importer.maxTextureSize = 1024;
            else if (assetPath.Contains("/Map/"))
            {
                importer.alphaIsTransparency = true;
                if (assetPath.Contains("parchment_bg"))
                    importer.maxTextureSize = 2048;
                else
                    importer.maxTextureSize = 128;
            }
            else if (assetPath.Contains("/Portraits/"))
                importer.maxTextureSize = 512;
            else
                importer.maxTextureSize = 512;
        }
    }
}
