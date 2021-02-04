using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TakeScreenshot : MonoBehaviour
{
    private static string path;
    private static string filename;
    private static int counter;
    public static List<Shader> shaderList = new List<Shader>();

    private void Start()
    {
        counter = 0;
        filename = "Capture";
        path = Application.dataPath + "/../Images/";
        //path = Application.dataPath + "/../RGB/";
        Directory.CreateDirectory(path); //creates directory
        Directory.CreateDirectory(path + "RGB/"); //creates directory
        Directory.CreateDirectory(path + "semantic/"); //creates directory
        Directory.CreateDirectory(path + "depth/"); //creates directory
    }
    public static void CaptureScreenshot(Camera cam, int width, int height)
    {
        // Depending on your render pipeline, this may not work.
        var bak_cam_targetTexture = cam.targetTexture;
        var bak_cam_clearFlags = cam.clearFlags;
        var bak_RenderTexture_active = RenderTexture.active;
        var tex_default = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var tex_depth = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        // Must use 24-bit depth buffer to be able to fill background.
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grab_area = new Rect(0, 0, width, height);
        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Simple: use a clear background
        //cam.backgroundColor = Color.clear;
        //cam.Render();
        for (int i=0; i<shaderList.Count; i++)
        {
            cam.RenderWithShader(shaderList[i], "");
            if (shaderList[i].name == "Custom/Depthmap")
            {
                tex_depth.ReadPixels(grab_area, 0, 0);
                tex_depth.Apply();
                byte[] Shot = ImageConversion.EncodeToEXR(tex_depth);
                string savePath = path + "depth/" + filename + counter + ".exr";
                Debug.Log(savePath);
                File.WriteAllBytes(savePath, Shot);
            }
            else
            {
                tex_default.ReadPixels(grab_area, 0, 0);
                tex_default.Apply();
                byte[] Shot = ImageConversion.EncodeToPNG(tex_depth);
                string savePath;
                if (shaderList[i].name == "Custom/SemanticColors")
                {
                    savePath = path + "semantic/" + filename + counter + ".png";
                }
                else
                {
                    savePath = path + "RGB/" + filename + counter + ".png";
                }
                File.WriteAllBytes(savePath, Shot);
            }
            
        }
        //tex_transparent.ReadPixels(grab_area, 0, 0);
        //tex_transparent.Apply();
        // Encode the resulting output texture to a byte array then write to the file
        //byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
        //byte[] pngShot = ImageConversion.EncodeToEXR(tex_transparent);
        //File.WriteAllBytes(savePath, pngShot);
        cam.clearFlags = bak_cam_clearFlags;
        cam.targetTexture = bak_cam_targetTexture;
        RenderTexture.active = bak_RenderTexture_active;
        RenderTexture.ReleaseTemporary(render_texture);
        Texture2D.Destroy(tex_depth);
        Texture2D.Destroy(tex_default);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("screenshot taken");
            CaptureScreenshot(Camera.main, Screen.width, Screen.height);
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