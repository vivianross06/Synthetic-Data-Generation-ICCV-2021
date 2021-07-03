using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.IO;
using System.Collections.Generic;


public class TakeScreenshot : Screenshoter
{
    private string path;
    private string filename;
    private int counter;
    private List<ScreenShotType> screenshotList;
    private int frameNumber=0;

    private void Start()
    {
        string date;
	if(OL_GLOBAL_INFO.FTNAME == "")
		date = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
	else
		date = OL_GLOBAL_INFO.FTNAME;
        counter = 0;
        filename = OL_GLOBAL_INFO.SCREENSHOT_FILENAME;
        //path = Application.dataPath + "/../ol_output/";
        path = "D:/4Eyes/ol_output/";
        Directory.CreateDirectory(path); //creates directory
        path = path + date + "/";
        Directory.CreateDirectory(path);
    }

    public override void ResetCounter()
    {
        counter = 0;
    }



    public override void CaptureScreenshot(Camera cam, int width, int height)
    {
        string countString;
        if (!File.Exists(path + "intrinsics.txt"))
        {
            Matrix4x4 intrinsics = cam.projectionMatrix;
            var textWrite = File.CreateText(path + "intrinsics.txt");
            textWrite.WriteLine(intrinsics[0, 0] + " " + intrinsics[0, 1] + " " + intrinsics[0, 2] + " " + intrinsics[0, 3]);
            textWrite.WriteLine(intrinsics[1, 0] + " " + intrinsics[1, 1] + " " + intrinsics[1, 2] + " " + intrinsics[1, 3]);
            textWrite.WriteLine(intrinsics[2, 0] + " " + intrinsics[2, 1] + " " + intrinsics[2, 2] + " " + intrinsics[2, 3]);
            textWrite.WriteLine(intrinsics[3, 0] + " " + intrinsics[3, 1] + " " + intrinsics[3, 2] + " " + intrinsics[3, 3]);
            textWrite.Close();
        }
        string specificPath = path + OL_GLOBAL_INFO.SCENE_NAME + "/";
        if (!Directory.Exists(specificPath + "Parameters/"))
        {
            Directory.CreateDirectory(specificPath + "Parameters/");
        }
        filename = OL_GLOBAL_INFO.SCREENSHOT_FILENAME;
        screenshotList = OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES;
        // Depending on your render pipeline, this may not work.
        var bak_cam_targetTexture = cam.targetTexture;
        var bak_cam_clearFlags = cam.clearFlags;
        var bak_RenderTexture_active = RenderTexture.active;
        var tex_RGB = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var tex_DEPTH = new Texture2D(width, height, TextureFormat.R16, false, true);
        // Must use 24-bit depth buffer to be able to fill background.
        RenderTexture render_texture;
        var grab_area = new Rect(0, 0, width, height);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        // Simple: use a clear background
        //cam.backgroundColor = Color.clear;
        //cam.Render();
        countString = counter.ToString().PadLeft(4, '0');
        for (int i = 0; i < screenshotList.Count; i++)
        {
            if (screenshotList[i].formatType == FormatEnum.DepthMap)
            {
                //Read in fixed4 values from fragment shader
                render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBFloat);
            }
            else
            {
                render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            }
            RenderTexture.active = render_texture;
            cam.targetTexture = render_texture;
            if (screenshotList[i].shader != null)
                cam.RenderWithShader(screenshotList[i].shader, "");
            else
                cam.Render();
            string dir = screenshotList[i].directoryName;
            if ((dir[dir.Length - 1] != '/') && dir.Length != 0)
            {
                dir = dir + "/";
            }
            string dir2 = specificPath + dir;
            if (!Directory.Exists(dir2))
            {
                Directory.CreateDirectory(dir2); //creates directory
            }
            string extention = "";
            byte[] Shot = new byte[0];
            if (screenshotList[i].formatType == FormatEnum.RGB)
            {
                tex_RGB.ReadPixels(grab_area, 0, 0);
                tex_RGB.Apply();
                Shot = ImageConversion.EncodeToPNG(tex_RGB);
                extention = ".png";
            }
            else if (screenshotList[i].formatType == FormatEnum.DepthMap)
            {
                var tex_temp = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
                tex_temp.ReadPixels(grab_area, 0, 0);
                var data = tex_temp.GetRawTextureData<Color>();
                byte[] depth_data = new byte[2 * width * height];
                for (int j = 0; j < (width * height); j++)
                {
                    float dist = Mathf.Min(65535.0f, data[j][3] - 1.0f);
                    ushort depth = (ushort)(Mathf.RoundToInt(dist));
                    depth_data[j * 2] = (byte)(depth);
                    depth_data[(j * 2) + 1] = (byte)(depth >> 8);
                }
                Texture2D.DestroyImmediate(tex_temp);
                tex_DEPTH.LoadRawTextureData(depth_data);
                Shot = ImageConversion.EncodeToPNG(tex_DEPTH);
                extention = ".png";
            }
            RenderTexture.ReleaseTemporary(render_texture);
            string savePath = specificPath + dir + filename + countString + extention;
            File.WriteAllBytes(savePath, Shot);
        }
        //Matrix4x4 extrinsics = cam.worldToCameraMatrix;
        Matrix4x4 extrinsics = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, new Vector3(1, 1, 1));
        var extrinsicsWrite = File.CreateText(specificPath + "Parameters/extrinsics" + countString + ".txt");
        
        extrinsicsWrite.WriteLine(extrinsics[0, 0] + " " + extrinsics[0, 1] + " " + extrinsics[0, 2] + " " + extrinsics[0, 3]);
        extrinsicsWrite.WriteLine(extrinsics[1, 0] + " " + extrinsics[1, 1] + " " + extrinsics[1, 2] + " " + extrinsics[1, 3]);
        extrinsicsWrite.WriteLine(extrinsics[2, 0] + " " + extrinsics[2, 1] + " " + extrinsics[2, 2] + " " + extrinsics[2, 3]);
        extrinsicsWrite.WriteLine(extrinsics[3, 0] + " " + extrinsics[3, 1] + " " + extrinsics[3, 2] + " " + extrinsics[3, 3]);
        
        //extrinsicsWrite.WriteLine(cam.transform.position.x + " " + cam.transform.position.y+" "+cam.transform.position.z);
        //extrinsicsWrite.WriteLine(cam.transform.rotation.x+" "+cam.transform.rotation.y+" "+cam.transform.rotation.z+" "+cam.transform.rotation.w);
        extrinsicsWrite.Close();

        //tex_transparent.ReadPixels(grab_area, 0, 0);
        //tex_transparent.Apply();
        // Encode the resulting output texture to a byte array then write to the file
        //byte[] pngShot = ImageConversion.EncodeToPNG(tex_transparent);
        //byte[] pngShot = ImageConversion.EncodeToEXR(tex_transparent);
        //File.WriteAllBytes(savePath, pngShot);
        cam.clearFlags = bak_cam_clearFlags;
        cam.targetTexture = bak_cam_targetTexture;
        RenderTexture.active = bak_RenderTexture_active;
        Texture2D.DestroyImmediate(tex_RGB);
        Texture2D.DestroyImmediate(tex_DEPTH);
        counter++;
    }

    void Update()
    {
        frameNumber++;
    }

}
