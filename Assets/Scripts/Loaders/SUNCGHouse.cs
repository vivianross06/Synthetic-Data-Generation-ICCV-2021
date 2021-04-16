using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using System;
using UnitySUNCG;

public class SUNCGHouse : Loader
{
    public List<string> ids = new List<string>();
    private int index = 0;
    public House house;
    private GameObject houseObject;
    private GameObject navAgent;
    public string houseId = "00a13f6179b2ed9a1bc10eef9d5b3bf4";
    private NavMeshSurface navMeshSurface;
    private NavMeshBuildSettings agentSettings;
    private bool isLoaded = false;

    public void generateIDs()
    {
        string[] dir = Directory.GetDirectories(Config.SUNCG_HOME + "house/");
        for (int i=0; i<dir.Length; i++)
        {
            string[] folders = dir[i].Split('/');
            ids.Add(folders[folders.Length - 1]);
        }
    }

    public override void Load()
    {
        //string houseId = "000d939dc2257995adcb27483b04ad04";
        house = Scene.GetHouseById(houseId);
        houseObject = Scene.GetHouseObject(house);
        //houseObject.GetComponent<HouseLoader>().Load(true);
        houseObject.GetComponent<HouseLoader>().Load();

        navMeshSurface = houseObject.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
        agentSettings = NavMesh.CreateSettings();
        agentSettings.agentHeight = 1.5f;
        agentSettings.agentRadius = 0.1f;
        navMeshSurface.BuildNavMeshWithSettings(agentSettings);
        //navMeshSurface.BuildNavMesh();

        List<(Vector3, Vector3)> bboxlist = new List<(Vector3, Vector3)>();
        foreach (Level l in house.levels)
        {
            float[] min = l.bbox.min;
            float[] max = l.bbox.max;
            if (!HouseLoader.flip)
            {
                float temp = min[2];
                min[2] = max[2];
                max[2] = temp;
            }
            (Vector3, Vector3) levelBox;
            levelBox.Item1.x = min[0];
            levelBox.Item2.x = max[0];
            levelBox.Item1.y = min[1];
            levelBox.Item2.y = max[1];
            levelBox.Item1.z = min[2];
            levelBox.Item2.z = max[2];
            bboxlist.Add(levelBox);
        }
        navAgent = OL_GLOBAL_INFO.AGENT;
        navAgent.GetComponent<NavMeshAgent>().agentTypeID = agentSettings.agentTypeID;
        OL_GLOBAL_INFO.BBOX_LIST = bboxlist;
        isLoaded = true;
    }

    public override void LoadNextScene()
    {
        generateIDs();
        if (isLoaded == true)
        {
            Destroy(houseObject);
            //Destroy(navAgent);
            isLoaded = false;
        }
        string currentHouseId = ids[index++];
        house = Scene.GetHouseById(currentHouseId);
        houseObject = Scene.GetHouseObject(house);
        //houseObject.GetComponent<HouseLoader>().Load(true);
        houseObject.GetComponent<HouseLoader>().Load();

        navMeshSurface = houseObject.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
        agentSettings = NavMesh.CreateSettings();
        agentSettings.agentHeight = 1.5f;
        agentSettings.agentRadius = 0.1f;
        navMeshSurface.BuildNavMeshWithSettings(agentSettings);
        //navMeshSurface.BuildNavMesh();

        List<(Vector3, Vector3)> bboxlist = new List<(Vector3, Vector3)>();
        foreach (Level l in house.levels)
        {
            float[] min = l.bbox.min;
            float[] max = l.bbox.max;
            if (!HouseLoader.flip)
            {
                float temp = min[2];
                min[2] = max[2];
                max[2] = temp;
            }
            (Vector3, Vector3) levelBox;
            levelBox.Item1.x = min[0];
            levelBox.Item2.x = max[0];
            levelBox.Item1.y = min[1];
            levelBox.Item2.y = max[1];
            levelBox.Item1.z = min[2];
            levelBox.Item2.z = max[2];
            bboxlist.Add(levelBox);
        }
        navAgent = OL_GLOBAL_INFO.AGENT;
        navAgent.GetComponent<NavMeshAgent>().agentTypeID = agentSettings.agentTypeID;
        OL_GLOBAL_INFO.BBOX_LIST = bboxlist;
        isLoaded = true;
    }

}
