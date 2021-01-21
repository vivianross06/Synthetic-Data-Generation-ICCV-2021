using Dummiesman;
using System.IO;
using UnityEngine;
using MatterportUnity;

public class MatterportOBJ : MonoBehaviour
{
    public string house; //17DRP5sb8fy
    string error = string.Empty;
    GameObject loadedObject;

    private void Start()
    {
        string[] dir = Directory.GetDirectories(Config.MATTERPORT_HOME + house + "/matterport_mesh/");
        //string[] dir = Directory.GetDirectories(Application.dataPath + "/../../matterport/" + house + "/matterport_mesh/");
        string[] folders = dir[0].Split('/');
        string fullPath = dir[0] + "/" + folders[folders.Length - 1];
        if (loadedObject != null)
            Destroy(loadedObject);
        loadedObject = new OBJLoader().Load(fullPath+".obj", fullPath+".mtl");
        loadedObject.transform.Rotate(-90, 0, 0);
        loadedObject.transform.position = new Vector3(0, 0, 0);
        loadedObject.transform.localScale = new Vector3(1, 1, 1);

    }

    /*
    void OnGUI() {
        //objPath = GUI.TextField(new Rect(0, 0, 256, 32), objPath);

        //GUI.Label(new Rect(0, 0, 256, 32), "Obj Path:");
        //if(GUI.Button(new Rect(256, 32, 64, 32), "Load File"))
        //{
        //file path
        string fullPath = objPath + house + "/matterport_mesh/" + objFile + "/" + objFile + ".obj";
            if (!File.Exists(fullPath))
            {
                error = "File doesn't exist.";
            }else{
                if(loadedObject != null)
                    Destroy(loadedObject);
                loadedObject = new OBJLoader().Load(fullPath);
                error = string.Empty;
            }
        //}

        if(!string.IsNullOrWhiteSpace(error))
        {
            GUI.color = Color.red;
            GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
            GUI.color = Color.white;
        }
    }
    */
}
