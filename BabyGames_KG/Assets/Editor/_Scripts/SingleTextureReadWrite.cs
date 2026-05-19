#if UNITY_EDITOR
using UnityEditor;

public class SingleTextureReadWrite : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        if (importer.assetBundleName == "your_asset_bundle_name")
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            // Apply changes
            importer.SaveAndReimport();
        }
    }
}
#endif