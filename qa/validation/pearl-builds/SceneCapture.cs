using System;
using System.Linq;
using UnityEngine;

// Render the real game camera and UI without depending on a visible desktop window.
public static class SceneCapture
{
    public static Texture2D Capture()
    {
        var camera=Camera.main;
        if(!camera)throw new Exception("Missing gameplay camera");
        var overlays=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(c=>c.isActiveAndEnabled&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        var previousCameras=overlays.Select(c=>c.worldCamera).ToArray();
        var distances=overlays.Select(c=>c.planeDistance).ToArray();
        var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
        var target=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32);
        try {
            camera.targetTexture=target;
            for(int i=0;i<overlays.Length;i++){
                overlays[i].renderMode=RenderMode.ScreenSpaceCamera;
                overlays[i].worldCamera=camera;overlays[i].planeDistance=Mathf.Max(1,camera.nearClipPlane+.5f);
            }
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
            var result=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            result.ReadPixels(new Rect(0,0,1920,1080),0,0);result.Apply();return result;
        } finally {
            for(int i=0;i<overlays.Length;i++){
                overlays[i].renderMode=RenderMode.ScreenSpaceOverlay;
                overlays[i].worldCamera=previousCameras[i];overlays[i].planeDistance=distances[i];
            }
            Canvas.ForceUpdateCanvases();camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);
        }
    }
}
