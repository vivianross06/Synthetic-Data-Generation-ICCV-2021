using UnityEngine;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
    private string path;
    private string filename;
    private int counter;

    private void Start()
    {
        counter = 0;
        filename = "Capture";
        path = Application.dataPath + "/../RGB/";
        Directory.CreateDirectory(path); //creates directory
    }
    public static void CaptureScreenshot(Camera cam, int width, int height, string savePath)
    {
        // Depending on your render pipeline, this may not work.
        var bak_cam_targetTexture = cam.targetTexture;
        var bak_cam_clearFlags = cam.clearFlags;
        var bak_RenderTexture_active = RenderTexture.active;
        //var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var tex_transparent = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        // Must use 24-bit depth buffer to be able to fill background.
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grab_area = new Rect(0, 0, width, height);
        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Simple: use a clear background
        //cam.backgroundColor = Color.clear;
        cam.Render();
        tex_transparent.ReadPixels(grab_area, 0, 0);
        tex_transparent.Apply();
        // Encode the resulting output texture to a byte array then write to the file
        //byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
        byte[] pngShot = ImageConversion.EncodeToEXR(tex_transparent);
        File.WriteAllBytes(savePath, pngShot);
        cam.clearFlags = bak_cam_clearFlags;
        cam.targetTexture = bak_cam_targetTexture;
        RenderTexture.active = bak_RenderTexture_active;
        RenderTexture.ReleaseTemporary(render_texture);
        Texture2D.Destroy(tex_transparent);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("screenshot taken");
            CaptureScreenshot(Camera.main, Screen.width, Screen.height, path + filename + counter++ + ".exr");
        }
    }

    /*
    void Start()
    {
        counter = 0;
        filename = "Capture";
        path = Application.dataPath + "/../RGB/";
        Directory.CreateDirectory(path); //creates directory
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ScreenCapture.CaptureScreenshot(path + filename + counter++ + ".png");
        }
    }
    */
}