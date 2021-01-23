using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatterportUnity;
using System.IO;

public class MatterportRoomPLY : MonoBehaviour
{
    public bool useSemantic = false;
    private bool shaderFlag = false;
    private string currentShader;
    public string house;
    public List<bool> roomNumber = new List<bool>();
    private List<GameObject> roomObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    { 
        shaderFlag = useSemantic;
        if (useSemantic)
        {
            currentShader = "Custom/SemanticColors";

        }
        else
        {
            currentShader = "Custom/VertexColors";
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
            roomNumber.Add(true);

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
        if (shaderFlag != useSemantic)
        {
            if (useSemantic)
            {
                currentShader = "Custom/SemanticColors";

            }
            else
            {
                currentShader = "Custom/VertexColors";
            }
            foreach (GameObject room in roomObjects)
            {
                foreach (Transform child in room.transform)
                {
                    child.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(currentShader);
                }
            }
            shaderFlag = useSemantic;
        }
        for (int i = 0; i < roomNumber.Count; i++)
        {
            roomObjects[i].SetActive(roomNumber[i]);
        }
    }
}
