using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace MantisPunch {
// Explicit render target also works when the validation window is hidden.
public static class ReviewCapture {
 public static void Save(string path,int width=1280,int height=720){
  var camera=Object.FindFirstObjectByType<MantisDuel>()?.arenaCamera;
  if(!camera)camera=Camera.main;if(!camera)camera=Object.FindFirstObjectByType<Camera>();
  var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
  foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.1f;}
  Canvas.ForceUpdateCanvases();var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();var previous=RenderTexture.active;
  RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=target});RenderTexture.active=target;
  var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
  RenderTexture.active=previous;target.Release();Object.Destroy(target);Object.Destroy(image);
  foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
 }
}
}
