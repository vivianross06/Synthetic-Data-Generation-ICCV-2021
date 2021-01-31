using Dummiesman;
using System.IO;
using UnityEngine;
using CampRobertsUnity;

public class CampRoberts : MonoBehaviour
{
    public string LevelOfDetail; //17DRP5sb8fy
    string error = string.Empty;
    GameObject loadedObject;
    GameObject parentObject;

    private void Start()
    {
           parentObject = new GameObject("CR");
           string[] dir = Directory.GetDirectories(Config.CR_HOME);
          for (int i=0; i<dir.Length; i++) {
               string[] folders = dir[i].Split('/');
               string tileName = folders[folders.Length - 1];
               string fullPath = dir[i] + "/" + tileName + "_" + LevelOfDetail;
            //if (loadedObject != null)
            //    Destroy(loadedObject);
            try
            {
                loadedObject = new OBJLoader().Load(fullPath + ".obj", fullPath + ".mtl");
            }
            catch (FileNotFoundException e) {
                loadedObject = new GameObject(tileName);
                Debug.Log(e);
            }
               loadedObject.transform.Rotate(-90, 0, 0);
               //loadedObject.transform.position = new Vector3(0, 0, 0);
               loadedObject.transform.localScale = new Vector3(1, 1, 1);
               loadedObject.transform.SetParent(parentObject.transform, false);



       }

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
