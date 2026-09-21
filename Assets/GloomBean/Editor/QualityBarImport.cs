using UnityEditor;
using UnityEngine;
namespace GloomBean.Editor {
public sealed class QualityBarImport:AssetPostprocessor {
public override uint GetVersion(){return 1;}
void OnPreprocessTexture(){if(!assetPath.Contains("/Resources/QualityBar/"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.mipmapEnabled=false;t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Repeat;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=2048;t.npotScale=TextureImporterNPOTScale.None;t.alphaIsTransparency=true;t.isReadable=true;}
}}
