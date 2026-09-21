using UnityEditor;
using UnityEngine;
namespace GloomBean.Editor
{
    public sealed class VisualV6Import:AssetPostprocessor
    {
        public override uint GetVersion(){return 2;}
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("/Resources/VisualV6/"))return;
            var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.mipmapEnabled=false;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=2048;t.npotScale=TextureImporterNPOTScale.None;t.alphaIsTransparency=true;t.isReadable=true;
        }
    }
}
