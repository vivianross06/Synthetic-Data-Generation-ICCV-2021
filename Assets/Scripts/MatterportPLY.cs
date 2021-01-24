using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MatterportUnity;
//using Loaders;

public class MatterportPLY : MonoBehaviour
{
    private string currentShader;
    private GameObject parentObj;
    public string house;
    public enum ShaderEnum { RGB, SemanticShader, DepthMap };
    public ShaderEnum shaders;
    // Use this for initialization
    void Start()
    {
        currentShader = "Custom/VertexColors";

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
            mr.material = new Material(Shader.Find(currentShader));
        }
        parent.transform.Rotate(-90, 0, 0);
        parentObj = parent;

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
        foreach (Transform child in parentObj.transform)
        {
            child.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(currentShader);
        }
        /*
        if (shaderFlag != useSemantic) {
            if (useSemantic)
            {
                currentShader = "Custom/Depthmap";

            }
            else
            {
                currentShader = "Custom/VertexColors";
            }
            foreach (Transform child in parentObj.transform)
            {
                child.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(currentShader);
            }
            shaderFlag = useSemantic;
        }
        */
    }
}
