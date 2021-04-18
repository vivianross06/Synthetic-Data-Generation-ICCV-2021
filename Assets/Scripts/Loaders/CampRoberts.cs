using Dummiesman;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using OmniLoaderUnity;

public class CampRoberts : Loader
{
    public int LevelOfDetail = 16;
    string error = string.Empty;
    GameObject loadedObject;
    GameObject parentObject;

    private GameObject navAgent;
    private NavMeshSurface navMeshSurface;
    private NavMeshBuildSettings agentSettings;

    public override string GetDatasetDirectory()
    {
        return Config.CR_HOME;
    }

    public override void SetNextScene(string sceneID)
    {
        int lod_unclamped = LevelOfDetail+1;
        try
        {
            lod_unclamped = System.Convert.ToInt32(sceneID);
        }
        catch (System.FormatException e)
        {
            e.ToString();
        }
        if (LevelOfDetail == 22)
        {
            Destroy(OL_GLOBAL_INFO.AGENT);
        }
        LevelOfDetail = Mathf.Min(Mathf.Max(lod_unclamped, 16), 22);
    }

    public override GameObject Load()
    {
        parentObject = new GameObject("CR");
        string[] dir = Directory.GetDirectories(Config.CR_HOME);
        for (int i = 0; i < dir.Length; i++)
        {
            string[] folders = dir[i].Split('/');
            string tileName = folders[folders.Length - 1];
            string fullPath = dir[i] + "/" + tileName + "_L" + LevelOfDetail;
            //if (loadedObject != null)
            //    Destroy(loadedObject);
            try
            {
                loadedObject = new OBJLoader().Load(fullPath + ".obj", fullPath + ".mtl", Shader.Find("Unlit/Texture"));
            }
            catch (FileNotFoundException e)
            {
                int newLevel = LevelOfDetail;
                string newPath = fullPath;
                while (newLevel>15 && !System.IO.File.Exists(newPath + ".obj"))
                {
                    newLevel--;
                    newPath = dir[i] + "/" + tileName + "_L" + newLevel;
                }
                if (newLevel < 16)
                {
                    loadedObject = new GameObject(tileName);
                    Debug.Log(e);
                }
                else
                {
                    loadedObject = new OBJLoader().Load(newPath + ".obj", newPath + ".mtl", Shader.Find("Unlit/Texture"));
                    Debug.Log("Unable to find tile " + tileName + " level " + LevelOfDetail
                        + ", used level " + newLevel);
                }
            }
            loadedObject.transform.Rotate(-90, 0, 0);
            //loadedObject.transform.position = new Vector3(0, 0, 0);
            loadedObject.transform.localScale = new Vector3(1, 1, 1);
            loadedObject.transform.SetParent(parentObject.transform, false);

        }
        OL_GLOBAL_INFO.setLayerOfAll(parentObject, 8);

        navMeshSurface = parentObject.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
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

        bb.Item1 = bounds.min;
        bb.Item2 = bounds.max;
        bbl.Add(bb);
        OL_GLOBAL_INFO.BBOX_LIST = bbl;
        return parentObject;
    }

    /*public override void LoadNextScene()
    {
        parentObject = new GameObject("CR");
        string[] dir = Directory.GetDirectories(Config.CR_HOME);
        for (int i = 0; i < dir.Length; i++)
        {
            string[] folders = dir[i].Split('/');
            string tileName = folders[folders.Length - 1];
            string fullPath = dir[i] + "/" + tileName + "_L" + LevelOfDetail;
            //if (loadedObject != null)
            //    Destroy(loadedObject);
            try
            {
                loadedObject = new OBJLoader().Load(fullPath + ".obj", fullPath + ".mtl", Shader.Find("Unlit/Texture"));
            }
            catch (FileNotFoundException e)
            {
                int newLevel = LevelOfDetail;
                string newPath = fullPath;
                while (newLevel > 15 && !System.IO.File.Exists(newPath + ".obj"))
                {
                    newLevel--;
                    newPath = dir[i] + "/" + tileName + "_L" + newLevel;
                }
                if (newLevel < 16)
                {
                    loadedObject = new GameObject(tileName);
                    Debug.Log(e);
                }
                else
                {
                    loadedObject = new OBJLoader().Load(newPath + ".obj", newPath + ".mtl", Shader.Find("Unlit/Texture"));
                    Debug.Log("Unable to find tile " + tileName + " level " + LevelOfDetail
                        + ", used level " + newLevel);
                }
            }
            loadedObject.transform.Rotate(-90, 0, 0);
            //loadedObject.transform.position = new Vector3(0, 0, 0);
            loadedObject.transform.localScale = new Vector3(1, 1, 1);
            loadedObject.transform.SetParent(parentObject.transform, false);

        }
        OL_GLOBAL_INFO.setLayerOfAll(parentObject, 8);

        navMeshSurface = parentObject.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
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

        bb.Item1 = bounds.min;
        bb.Item2 = bounds.max;
        bbl.Add(bb);
        OL_GLOBAL_INFO.BBOX_LIST = bbl;
    }*/

}
