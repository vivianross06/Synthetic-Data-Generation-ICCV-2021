using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public enum FormatEnum { RGB, DepthMap };
public enum ModeEnum { Robot, Human };

[System.Serializable]
public struct ScreenShotType
{
    public Shader shader;
    public string directoryName;
    public FormatEnum formatType;
}

public class OmniLoader : MonoBehaviour
{
    [HideInInspector] public MonoScript AgentScript;
    [HideInInspector] public MonoScript LoaderScript; //Loader is responsible for NavMesh generation
    [HideInInspector] public Boolean loadAll; //determines whether to load every scene in directory or not
    [HideInInspector] public MonoScript ScreenshotScript;
    [HideInInspector] public List<ScreenShotType> scs = new List<ScreenShotType>(0);
    [HideInInspector] public uint agentWaypoints = 40;
    [HideInInspector] public float stepDistance = 1.0f;
    [HideInInspector] public ModeEnum flythroughMode;

    // Start is called before the first frame update
    void Start()
    {
        GameObject agentObj = new GameObject("Agent");
        agentObj.SetActive(false);
        OL_GLOBAL_INFO.AGENT = agentObj;
        OL_GLOBAL_INFO.SCREENSHOT_PROPERTIES = scs;
        OL_GLOBAL_INFO.TOTAL_POINTS = Convert.ToInt32(agentWaypoints);
        OL_GLOBAL_INFO.DISTANCE_BETWEEN_SCREENSHOTS = stepDistance;
        OL_GLOBAL_INFO.FLYTHROUGH_MODE = flythroughMode;
        /*if (flythroughMode == ModeEnum.Robot)
        {
            OL_GLOBAL_INFO.MAX_ROTATION = 90.0f;
        }else if (flythroughMode == ModeEnum.Human)
        {
            OL_GLOBAL_INFO.MAX_ROTATION = 40.0f;
        }*/

        if (AgentScript != null)
            agentObj.AddComponent(AgentScript.GetClass());
        if (ScreenshotScript != null)
            agentObj.AddComponent(ScreenshotScript.GetClass());
        NavMeshAgent nma = agentObj.AddComponent<NavMeshAgent>();
        nma.enabled = false;
        //modify desired values of NavMeshAgent component here.
        //example: nma.speed = 3.5f;
        nma.speed = 0.9f;
        nma.acceleration = 1.2f;
        if (AgentScript != null)
        {
            GameObject camera = Camera.main.gameObject;
            camera.transform.SetParent(agentObj.transform);
            camera.transform.localPosition = new Vector3(0, 1.5f, 0);
            camera.transform.localRotation = Quaternion.identity;
        }
        if (LoaderScript != null)
        {
            if (loadAll == false)
            {
                ((Loader)GetComponent(LoaderScript.GetClass())).Load();
            }
            else
            {
                ((Loader)GetComponent(LoaderScript.GetClass())).LoadNextScene();
            }
        }
        if (AgentScript != null)
            ((Agent)agentObj.GetComponent(AgentScript.GetClass())).StartAgent(OL_GLOBAL_INFO.BBOX_LIST);

    }

    // Update is called once per frame
    void Update()
    {
        if (loadAll == true)
        {
            GameObject agent = GameObject.Find("Agent");
            SimpleAgent agent1 = agent.GetComponent<SimpleAgent>();
            bool isDone = agent1.done;
            if (isDone == true)
            {
                ((Loader)GetComponent(LoaderScript.GetClass())).LoadNextScene();
            }

        }
    }
}


public class Loader : MonoBehaviour
{
    public virtual void Load()
    { }
    public virtual void LoadNextScene()
    { }
}

public class Agent : MonoBehaviour
{
    public virtual void StartAgent(List<(Vector3, Vector3)> bboxlist)
    { }
}

public class Screenshoter : MonoBehaviour
{
    public virtual void CaptureScreenshot(Camera cam, int width, int height)
    { }
}

public static class OL_GLOBAL_INFO
{
    public static GameObject AGENT;
    public static List<ScreenShotType> SCREENSHOT_PROPERTIES;
    public static string SCREENSHOT_FILENAME = "Capture";
    public static int TOTAL_POINTS = 40;
    public static float DISTANCE_BETWEEN_SCREENSHOTS = 0.1f;
    public static float MAX_TIME_BETWEEN_POINTS = 60.0f;
    public static float MAX_ROTATION_X = 40.0f;
    public static float MAX_ROTATION_Y = 80.0f;
    public static float CAM_ROTATION_DURATION = 0.5f;
    public static float CAM_ROTATION_FREQUENCY = 0.5f;
    public static ModeEnum FLYTHROUGH_MODE;
    public static List<(Vector3, Vector3)> BBOX_LIST;

    public static void setLayerOfAll(GameObject root, int layer) {
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            setLayerOfAll(child.gameObject, layer);
        }
    }
}
