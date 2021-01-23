using UnityEngine;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
    private string path;
    private string filename;
    private int counter;

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
}