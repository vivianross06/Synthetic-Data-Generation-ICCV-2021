using UnityEngine;
using System.Collections;
using MatterportUnity;
//using Loaders;

public class MatterportPLY : MonoBehaviour
{
    public string house;
    // Use this for initialization
    void Start()
    {
        PlyLoader loader = new PlyLoader();
        string path = Config.MATTERPORT_HOME + house + "/house_segmentations/" + house + ".ply";
        //Mesh [] mesh = loader.load("2PTC_EI_bs_1.ply");
        Mesh[] mesh = loader.load(path);

        GameObject parent = new GameObject("plyObject");
        for (int i = 0; i != mesh.Length; ++i)
        {
            GameObject g = new GameObject();
            g.transform.parent = parent.transform;
            mesh[i].name = g.name = "mesh" + i;
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = mesh[i];
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Custom/VertexColors"));
        }
        parent.transform.Rotate(-90, 0, 0);
        for (int i=0; i<100; i++)
        {
            Debug.Log(mesh[92].uv[i]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
