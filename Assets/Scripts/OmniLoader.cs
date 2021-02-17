using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public enum FileEnum { PNG, EXR };

[System.Serializable]
public struct ScreenShotType
{
    public Shader shader;
    public string directoryName;
    public FileEnum fileType;
}

public class OmniLoader : MonoBehaviour
{
    [HideInInspector] public MonoScript Agent;
    [HideInInspector] public MonoScript Loader; //Loader is responsible for NavMesh generation
    [HideInInspector] public MonoScript ScreenshotScript;
    [HideInInspector] public List<ScreenShotType> scs = new List<ScreenShotType>(0);

    // Start is called before the first frame update
    void Start()
    {
        GameObject agentObj = new GameObject("Agent");
        agentObj.SetActive(false);
        OL_GLOBAL_INFO.AGENT = agentObj;
        OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES = scs;
        agentObj.AddComponent(Agent.GetClass());
        agentObj.AddComponent(ScreenshotScript.GetClass());
        NavMeshAgent nma = agentObj.AddComponent<NavMeshAgent>();
        nma.enabled = false;
        //modify desired values of NavMeshAgent component here.
        //example: nma.speed = 3.5f;
        GameObject camera = Camera.main.gameObject;
        camera.transform.SetParent(agentObj.transform);
        camera.transform.localPosition = new Vector3(0, 1.5f, 0);
        camera.transform.localRotation = Quaternion.identity;

        ((MonoBehaviour)GetComponent(Loader.GetClass())).Invoke("Load", 0.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public static class OL_GLOBAL_INFO
{
    public static GameObject AGENT;
    public static List<ScreenShotType> SCREENSHOT_PROPERTIES;
    public static string SCREENSHOT_FILENAME = "Capture";
}
