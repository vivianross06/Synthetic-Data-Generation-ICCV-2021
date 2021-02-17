using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OmniLoaderUnity;
using System.IO;

public class MatterportRoomPLY : MonoBehaviour
{
    public enum ShaderEnum { RGB, SemanticShader, DepthMap };
    public ShaderEnum shaders;
    private string currentShader;
    public string house;
    public List<bool> roomNumber = new List<bool>();
    private bool defaultValue = true; //Do we show a room if it is not specified whether we want to show it or not?
    private List<GameObject> roomObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
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
                g.transform.parent = room.transform;
                mesh[i].name = g.name = "mesh" + i;
                MeshFilter mf = g.AddComponent<MeshFilter>();
                mf.mesh = mesh[i];
                MeshRenderer mr = g.AddComponent<MeshRenderer>();
                mr.material = new Material(Shader.Find(currentShader));
            }
            roomObjects.Add(room);
        }
        parent.transform.Rotate(-90, 0, 0);
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
}
