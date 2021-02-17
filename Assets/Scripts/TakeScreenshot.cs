using UnityEngine;
using System.IO;
using System.Collections.Generic;


public class TakeScreenshot : MonoBehaviour
{
    private string path;
    private string filename;
    private int counter;
    private List<ScreenShotType> screenshotList;

    private void Start()
    {
        counter = 0;
        filename = OL_GLOBAL_INFO.SCREENSHOT_FILENAME;
        path = Application.dataPath + "/../Images/";
        Directory.CreateDirectory(path); //creates directory
    }
    public void CaptureScreenshot(Camera cam, int width, int height)
    {
        filename = OL_GLOBAL_INFO.SCREENSHOT_FILENAME;
        path = Application.dataPath + "/../Images/";
        screenshotList = OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES;
        // Depending on your render pipeline, this may not work.
        var bak_cam_targetTexture = cam.targetTexture;
        var bak_cam_clearFlags = cam.clearFlags;
        var bak_RenderTexture_active = RenderTexture.active;
        var tex_PNG = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var tex_EXR = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        // Must use 24-bit depth buffer to be able to fill background.
        var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var grab_area = new Rect(0, 0, width, height);
        RenderTexture.active = render_texture;
        cam.targetTexture = render_texture;
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Simple: use a clear background
        //cam.backgroundColor = Color.clear;
        //cam.Render();
        for (int i=0; i< screenshotList.Count; i++)
        {
            if (screenshotList[i].shader != null)
                cam.RenderWithShader(screenshotList[i].shader, "");
            else
                cam.Render();
            string dir = screenshotList[i].directoryName;
            if((dir[dir.Length-1] != '/') && dir.Length != 0)
			{
                dir = dir + "/";
			}
            string dir2 = path + dir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir2); //creates directory
            }
            string extention = "";
            byte[] Shot = new byte[0];
            if (screenshotList[i].fileType == FileEnum.PNG)
            {
                tex_PNG.ReadPixels(grab_area, 0, 0);
                tex_PNG.Apply();
                Shot = ImageConversion.EncodeToPNG(tex_PNG);
                extention = ".png";
            }
            else if(screenshotList[i].fileType == FileEnum.EXR)
            {
                tex_EXR.ReadPixels(grab_area, 0, 0);
                tex_EXR.Apply();
                Shot = ImageConversion.EncodeToPNG(tex_EXR);
                extention = ".exr";
            }
            string savePath = path + dir + filename + counter + extention;
            File.WriteAllBytes(savePath, Shot);


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
        Texture2D.Destroy(tex_PNG);
        Texture2D.Destroy(tex_EXR);
        counter++;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("screenshot taken");
            CaptureScreenshot(Camera.main, Screen.width, Screen.height);
        }*/
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