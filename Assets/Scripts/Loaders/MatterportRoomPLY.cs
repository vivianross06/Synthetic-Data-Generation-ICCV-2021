﻿using Dummiesman;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using OmniLoaderUnity;

public class MatterportRoomPLY : Loader
{
    public enum ShaderEnum { RGB, SemanticShader, DepthMap };
    public ShaderEnum shaders;
    private string currentShader;
    public string house;
    public List<bool> roomNumber = new List<bool>();
    private bool defaultValue = true; //Do we show a room if it is not specified whether we want to show it or not?
    private List<GameObject> roomObjects = new List<GameObject>();

    private GameObject navAgent;
    private NavMeshSurface navMeshSurface;
    private NavMeshBuildSettings agentSettings;
    // Start is called before the first frame update
    public override void Load()
    {
        if (shaders == ShaderEnum.RGB)
        {
            currentShader = "Custom/VertexColors";
        }
        else if (shaders == ShaderEnum.SemanticShader)
        {
            currentShader = "Custom/SemanticColors";
        }
        else if (shaders == ShaderEnum.DepthMap)
        {
            currentShader = "Custom/Depthmap";
        }

        string path = Config.MATTERPORT_HOME + house + "/region_segmentations/";
        string[] files = Directory.GetFiles(path,"*.ply");

        PlyLoader loader = new PlyLoader();
        GameObject parent = new GameObject("roomPlyObject");
        GameObject room;

        for (int j = 0; j < files.Length; j++)
        {
            path = files[j];
            Mesh[] mesh = loader.load(path);
            if(roomNumber.Count == j)
                roomNumber.Add(defaultValue);

            room=new GameObject("room" + j) ;
            room.transform.parent = parent.transform;
            for (int i = 0; i != mesh.Length; ++i)
            {
                GameObject g = new GameObject();
                g.transform.Rotate(-90, 0, 0);
                g.transform.parent = room.transform;
                mesh[i].name = g.name = "mesh" + i;
                MeshFilter mf = g.AddComponent<MeshFilter>();
                mf.mesh = mesh[i];
                MeshRenderer mr = g.AddComponent<MeshRenderer>();
                mr.material = new Material(Shader.Find(currentShader));
            }
            roomObjects.Add(room);
        }

        //code where we make the navmesh and set the agent off.
        GameObject fromObj = loadObj();
        navMeshSurface = parent.AddComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("NavMeshLayer");
        //^good
        agentSettings = UnityEngine.AI.NavMesh.CreateSettings();
        agentSettings.agentHeight = 1.5f;
        agentSettings.agentRadius = 0.1f;
        navMeshSurface.BuildNavMeshWithSettings(agentSettings);
        navAgent = OL_GLOBAL_INFO.AGENT;
        navAgent.GetComponent<UnityEngine.AI.NavMeshAgent>().agentTypeID = agentSettings.agentTypeID;



        List<(Vector3, Vector3)> bbl = new List<(Vector3, Vector3)>();
        (Vector3, Vector3) bb;

        Renderer[] renderers = (Renderer[])Object.FindObjectsOfType(typeof(Renderer));
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.layer == 8)
                bounds.Encapsulate(renderer.bounds);
        }
        bb.Item1 = bounds.min;
        bb.Item2 = bounds.max;
        bbl.Add(bb);

        fromObj.SetActive(false);

        OL_GLOBAL_INFO.BBOX_LIST = bbl;
    }


    // Update is called once per frame
    void Update()
    {
        if (shaders == ShaderEnum.RGB)
        {
            currentShader = "Custom/VertexColors";
        }
        else if (shaders == ShaderEnum.SemanticShader)
        {
            currentShader = "Custom/SemanticColors";
        }
        else if (shaders == ShaderEnum.DepthMap)
        {
            currentShader = "Custom/Depthmap";
        }
        foreach (GameObject room in roomObjects)
        {
            foreach (Transform child in room.transform)
            {
                child.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(currentShader);
            }
        }
        for (int i = 0; i < roomNumber.Count; i++)
        {
            roomObjects[i].SetActive(roomNumber[i]);
        }
    }

    private GameObject loadObj()
    {
        string[] dir = Directory.GetDirectories(Config.MATTERPORT_HOME + house + "/matterport_mesh/");
        //string[] dir = Directory.GetDirectories(Application.dataPath + "/../../matterport/" + house + "/matterport_mesh/");
        string[] folders = dir[0].Split('/');
        string fullPath = dir[0] + "/" + folders[folders.Length - 1];
        GameObject loadedObject = new OBJLoader().Load(fullPath + ".obj", fullPath + ".mtl", Shader.Find("Unlit/Texture"));
        loadedObject.transform.Rotate(-90, 0, 0);
        loadedObject.transform.position = new Vector3(0, 0, 0);
        loadedObject.transform.localScale = new Vector3(1, 1, 1);
        GameObject rotFix = new GameObject("objObject");
        loadedObject.transform.SetParent(rotFix.transform);
        OL_GLOBAL_INFO.setLayerOfAll(loadedObject, 8);
        return rotFix;
    }
}
