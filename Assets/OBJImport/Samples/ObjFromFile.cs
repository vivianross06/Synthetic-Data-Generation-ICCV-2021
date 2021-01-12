using Dummiesman;
using System.IO;
using UnityEngine;

public class ObjFromFile : MonoBehaviour
{
    public string house; //17DRP5sb8fy
    public string objFile; //bed1a77d92d64f5cbbaaae4feed64ec1
    string error = string.Empty;
    GameObject loadedObject;

    private void Start()
    {
        //make path match matterport irectory
        string fullPath = Application.dataPath + "/../../matterport/" + house + "/matterport_mesh/" + objFile + "/" + objFile + ".obj";
        if (loadedObject != null)
            Destroy(loadedObject);
        loadedObject = new OBJLoader().Load(fullPath);

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
