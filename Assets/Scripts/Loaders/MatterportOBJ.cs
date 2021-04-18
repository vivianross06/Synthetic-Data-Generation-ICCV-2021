using Dummiesman;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using OmniLoaderUnity;

public class MatterportOBJ : Loader
{
    private string house= "17DRP5sb8fy"; //17DRP5sb8fy
    string error = string.Empty;
    GameObject loadedObject;

    private GameObject navAgent;
    private NavMeshSurface navMeshSurface;
    private NavMeshBuildSettings agentSettings;

    public override string GetDatasetDirectory()
    {
        return Config.MATTERPORT_HOME;
    }

    public override void SetNextScene(string sceneID)
    {
        house = sceneID;
    }

    public override GameObject Load()
    {
        string[] dir = Directory.GetDirectories(Config.MATTERPORT_HOME + house + "/matterport_mesh/");
        //string[] dir = Directory.GetDirectories(Application.dataPath + "/../../matterport/" + house + "/matterport_mesh/");
        string[] folders = dir[0].Split('/');
        string fullPath = dir[0] + "/" + folders[folders.Length - 1];
        if (loadedObject != null)
            Destroy(loadedObject);
        loadedObject = new OBJLoader().Load(fullPath+".obj", fullPath+".mtl", Shader.Find("Unlit/Texture"));
        loadedObject.transform.Rotate(-90, 0, 0);
        loadedObject.transform.position = new Vector3(0, 0, 0);
        loadedObject.transform.localScale = new Vector3(1, 1, 1);

        GameObject rotFix = new GameObject("objObject");
        loadedObject.transform.SetParent(rotFix.transform);
        OL_GLOBAL_INFO.setLayerOfAll(rotFix, 8);

        navMeshSurface = rotFix.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
        //^good
        agentSettings = NavMesh.CreateSettings();
        agentSettings.agentHeight = 1.5f;
        agentSettings.agentRadius = 0.1f;
        navMeshSurface.BuildNavMeshWithSettings(agentSettings);
        navAgent = OL_GLOBAL_INFO.AGENT;
        navAgent.GetComponent<NavMeshAgent>().agentTypeID = agentSettings.agentTypeID;




        List<(Vector3, Vector3)> bbl = new List<(Vector3, Vector3)>();
        (Vector3, Vector3) bb;

        Renderer[] renderers = (Renderer[])Object.FindObjectsOfType(typeof(Renderer));
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        //Vector3[] verts = NavMesh.CalculateTriangulation().vertices;
        //int[] ids = NavMesh.CalculateTriangulation().indices;
        //Mesh mesh = new Mesh();
        //mesh.vertices = verts;
        //mesh.triangles = ids;

        bb.Item1 = bounds.min;
        bb.Item2 = bounds.max;
        bbl.Add(bb);
        OL_GLOBAL_INFO.BBOX_LIST = bbl;

        return rotFix;
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
