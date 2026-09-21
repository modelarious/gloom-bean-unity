using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    /// <summary>Graphics/audio component evidence, never a gameplay or human-acceptance certificate.</summary>
    public static class NativePresentationChecks
    {
        static Texture2D ReadTexture(Texture texture)
        {
            var old=RenderTexture.active;var rt=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);
            Graphics.Blit(texture,rt);RenderTexture.active=rt;var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            copy.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);copy.Apply();RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);return copy;
        }
        static string Hash(byte[] bytes){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static void Wav(string path,float[] samples)
        {
            using(var f=new BinaryWriter(File.Create(path))){int bytes=samples.Length*2;f.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));f.Write(36+bytes);f.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));f.Write(16);f.Write((short)1);f.Write((short)1);f.Write(CampaignScore.Rate);f.Write(CampaignScore.Rate*2);f.Write((short)2);f.Write((short)16);f.Write(System.Text.Encoding.ASCII.GetBytes("data"));f.Write(bytes);foreach(float s in samples)f.Write((short)Mathf.RoundToInt(Mathf.Clamp(s,-1,1)*32767));}
        }
        public static IEnumerator Run(GameRoot game,Action<string,bool,string> check)
        {
            string dir=Path.Combine(game.reportDirectory,"Presentation");Directory.CreateDirectory(dir);
            var v6Textures=Resources.LoadAll<Texture2D>("VisualV6");check("presentation.v6.texture2d-resource-contract",V6Art.RequiredAssets.All(n=>v6Textures.Any(t=>t.name==n))&&v6Textures.All(t=>t.filterMode==FilterMode.Point),"Texture2D="+v6Textures.Length);

            var gba=game.GetComponent<GbaDisplay>();check("presentation.gba.native-render-surface",gba&&gba.Frame&&gba.Frame.width==240&&gba.Frame.height==160&&gba.Frame.filterMode==FilterMode.Point&&gba.Frame.antiAliasing==1,"Actual native render target");
            var independent=GbaPixels.TextTexture("WORD",30,8,1,true,true);int opaque=independent.GetPixels32().Count(c=>c.a>0);
            for(int i=0;i<390;i++)GbaPixels.TextTexture("CACHE "+i,72,8);yield return null;
            check("presentation.gba.world-lettering-survives-ui-cache-turnover",independent&&opaque>25&&independent.GetPixels32().Count(c=>c.a>0)==opaque,"visible pixels="+opaque);
            var glyphGpu=ReadTexture(independent);check("presentation.gba.owned-lettering-visible-on-gpu",glyphGpu.GetPixels32().Count(c=>c.a>0)>25,"Actual GPU sign pixels");UnityEngine.Object.Destroy(glyphGpu);UnityEngine.Object.Destroy(independent);
            yield return QualityBarMotionChecks.Run(game,check);
            var hashes=new HashSet<string>();var sheet=new Texture2D(6*64,3*64,TextureFormat.RGBA32,false);sheet.SetPixels32(new Color32[6*64*3*64]);
            for(int i=0;i<17;i++){
                var form=i<2?HostKind.None:(HostKind)(i-1);var sprite=HostPixelArt.Host(form,i!=0,0);var t=ReadTexture(sprite.texture);var pixels=t.GetPixels32();int visible=pixels.Count(c=>c.a>0);
                var bytes=t.EncodeToPNG();hashes.Add(Hash(bytes));check("presentation.host-"+i,visible>300&&visible<4096&&sprite.texture.filterMode==FilterMode.Point,"visible="+visible);
                sheet.SetPixels32((i%6)*64,(2-i/6)*64,64,64,pixels);UnityEngine.Object.Destroy(t);
            }
            sheet.Apply();File.WriteAllBytes(Path.Combine(dir,"original-host-and-fifteen-forms.png"),sheet.EncodeToPNG());UnityEngine.Object.Destroy(sheet);
            check("presentation.seventeen-distinct-normal-corrupt-and-form-assets",hashes.Count==17,"distinct="+hashes.Count);
            hashes.Clear();for(int i=1;i<=15;i++){var t=ReadTexture(HostPixelArt.Tenant((HostKind)i).texture);hashes.Add(Hash(t.EncodeToPNG()));File.WriteAllBytes(Path.Combine(dir,"tenant-"+i.ToString("00")+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);}
            check("presentation.fifteen-diegetic-creature-assets",hashes.Count==15,"distinct="+hashes.Count);
            for(int w=0;w<=5;w++){var t=ReadTexture(SceneryArt.Get(w).texture);File.WriteAllBytes(Path.Combine(dir,"world-"+w+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);}
            for(int w=1;w<=5;w++){var t=ReadTexture(HostPixelArt.Boss(w).texture);File.WriteAllBytes(Path.Combine(dir,"boss-"+w+".png"),t.EncodeToPNG());UnityEngine.Object.Destroy(t);}
            var audio=new List<string>{"theme,returned,frames,peak,rms,sha256"};hashes.Clear();
            for(int w=0;w<8;w++)for(int state=0;state<2;state++){
                var pcm=CampaignScore.Compose(w,state==1);bool finite=pcm.All(v=>!float.IsNaN(v)&&!float.IsInfinity(v));float peak=pcm.Max(v=>Mathf.Abs(v));double rms=Math.Sqrt(pcm.Select(v=>(double)v*v).Average());var bytes=new byte[pcm.Length*4];Buffer.BlockCopy(pcm,0,bytes,0,bytes.Length);string sha=Hash(bytes);hashes.Add(sha);
                check("presentation.score-"+w+"-"+state,finite&&pcm.Length>200000&&peak<.66f&&rms>.015&&rms<.25,"frames="+pcm.Length+" peak="+peak+" rms="+rms);
                audio.Add(w+","+state+","+pcm.Length+","+peak.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+rms.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+sha);
                if(state==0)Wav(Path.Combine(dir,"score-"+w+".wav"),pcm);yield return null;
            }
            File.WriteAllLines(Path.Combine(dir,"score-analysis.csv"),audio);check("presentation.sixteen-distinct-original-score-variants",hashes.Count==16,"distinct="+hashes.Count);
            check("presentation.menu-hook-installed",game.PresentationBackground!=null&&game.GetComponent<CampaignPresentation>()!=null,"Actual GameRoot callback");
            File.WriteAllText(Path.Combine(dir,"SCOPE.txt"),"Actual GPU texture readback and synthesized PCM. Artwork and audio assertions establish distinct nonempty finite assets and installed rendering hooks, not beauty, music taste, controller feel or a completed campaign. Gameplay ending images are captured separately after actual earned boss victories.");
        }
        public static IEnumerator Screen(string path)
        {
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(path,image.EncodeToPNG());UnityEngine.Object.Destroy(image);
        }
    }
}
